using System;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Yearly death risk = (Gompertz(age) + stage extra + conditions) × health multiplier × healthcare multiplier,
    /// converted to a weekly probability. Death is certain at the maximum age.
    /// </summary>
    public sealed class MortalityModel
    {
        private const float WeeksPerYear = GameCalendar.WeeksPerYear;
        private const float SeverityScale = StatBounds.Max;

        /// <summary>
        /// Creates the model.
        /// </summary>
        /// <param name="settings">Mortality settings.</param>
        public MortalityModel(MortalitySettings settings)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>Mortality settings.</summary>
        public MortalitySettings Settings { get; }

        /// <summary>
        /// Explains the death risk of a character this week.
        /// </summary>
        /// <param name="character">Character.</param>
        /// <param name="week">Current week.</param>
        /// <returns>The breakdown.</returns>
        public MortalityBreakdown Explain(Character character, GameDate week)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            int age = character.AgeAt(week);
            float ageRisk = Settings.AnnualAtReference * (float)Math.Exp(Settings.GrowthPerYear * (age - Settings.ReferenceAge));
            float stageRisk = character.LifeStage(week).ExtraAnnualMortality;
            float conditionRisk = character.Conditions.Sum(c => character.Catalogs.HealthConditions.Get(c.ConditionId).AnnualMortality * c.Severity / SeverityScale);
            float health = Multiplier(character.Stats.GetValue(StatIds.Health), Settings.HealthAtZero, Settings.HealthAtFull);
            float healthcare = Multiplier(character.LocationModifiers().Healthcare ?? MortalitySettings.Neutral, Settings.HealthcareAtZero, Settings.HealthcareAtFull);

            if (age >= Settings.MaxAge)
            {
                return new MortalityBreakdown(age, ageRisk, stageRisk, conditionRisk, health, healthcare, 1f, 1f);
            }
            float annual = Math.Max(0f, Math.Min(1f, (ageRisk + stageRisk + conditionRisk) * health * healthcare));
            float weekly = (float)(1.0 - Math.Pow(1.0 - annual, 1.0 / WeeksPerYear));
            return new MortalityBreakdown(age, ageRisk, stageRisk, conditionRisk, health, healthcare, annual, Math.Min(Settings.MaxWeeklyProbability, weekly));
        }

        /// <summary>
        /// Returns the probability of dying this week.
        /// </summary>
        /// <param name="character">Character.</param>
        /// <param name="week">Current week.</param>
        /// <returns>Probability, 0-1.</returns>
        public float WeeklyProbability(Character character, GameDate week) => Explain(character, week).Weekly;

        /// <summary>
        /// Returns the yearly death probability of a lightly simulated person (an NPC): the age curve and healthcare only,
        /// with neutral health.
        /// </summary>
        /// <param name="age">Age in years.</param>
        /// <param name="healthcare">Healthcare quality 0-100, or null for neutral.</param>
        /// <returns>Probability, 0-1.</returns>
        public float AnnualForAge(int age, float? healthcare)
        {
            if (age >= Settings.MaxAge)
            {
                return 1f;
            }
            float ageRisk = Settings.AnnualAtReference * (float)Math.Exp(Settings.GrowthPerYear * (age - Settings.ReferenceAge));
            float care = Multiplier(healthcare ?? MortalitySettings.Neutral, Settings.HealthcareAtZero, Settings.HealthcareAtFull);
            return Math.Max(0f, Math.Min(1f, ageRisk * care));
        }

        // Log-linear: neutral (50) gives ×1, 0 gives atZero, 100 gives atFull.
        private static float Multiplier(float value, float atZero, float atFull)
        {
            float clamped = StatBounds.Clamp(value);
            if (clamped <= MortalitySettings.Neutral)
            {
                return (float)Math.Exp(Math.Log(atZero) * (MortalitySettings.Neutral - clamped) / MortalitySettings.Neutral);
            }
            return (float)Math.Exp(Math.Log(atFull) * (clamped - MortalitySettings.Neutral) / MortalitySettings.Neutral);
        }
    }
}
