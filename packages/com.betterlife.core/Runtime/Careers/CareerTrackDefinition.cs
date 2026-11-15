using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Luck;
using BetterLife.Core.Rules;
using BetterLife.Core.Time.Budget;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// A career ladder: requirements, hiring odds, hours, stress, income behaviour, risks and levels.
    /// </summary>
    public sealed class CareerTrackDefinition : IDefinition
    {
        /// <summary>
        /// Creates the track.
        /// </summary>
        /// <param name="id">Unique id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="category">Category.</param>
        /// <param name="premium">Premium (deeper, never better paid).</param>
        /// <param name="requirement">Condition to apply, or null.</param>
        /// <param name="hiring">Base hiring chance.</param>
        /// <param name="weeklyHours">Work hours per week.</param>
        /// <param name="performanceSkillId">Skill that counts towards performance, or null.</param>
        /// <param name="skills">Skills practiced per work hour.</param>
        /// <param name="stressPerHour">Stress added per work hour.</param>
        /// <param name="incomeVolatility">Monthly income noise (0 = fixed salary).</param>
        /// <param name="marketBeta">Sensitivity of income to the economy's stock outlook.</param>
        /// <param name="layoffSensitivity">Multiplier of the economy's layoff chance.</param>
        /// <param name="pensionRate">Share of the last salary paid as pension after retirement.</param>
        /// <param name="risk">Monthly risk, or null.</param>
        /// <param name="levels">Ladder from the entry level up; at least one.</param>
        public CareerTrackDefinition(string id, string nameKey, CareerCategory category, bool premium, ICondition requirement, ChanceSpec hiring,
            int weeklyHours, string performanceSkillId, IEnumerable<ActivitySkillEffect> skills, float stressPerHour, float incomeVolatility,
            float marketBeta, float layoffSensitivity, float pensionRate, CareerRisk risk, IEnumerable<CareerLevelDefinition> levels)
        {
            if (weeklyHours < 1 || weeklyHours > TimeBudget.HoursPerWeek)
            {
                throw new ArgumentOutOfRangeException(nameof(weeklyHours), weeklyHours, "Weekly hours must be within 1-168.");
            }
            if (stressPerHour < 0f || incomeVolatility < 0f || layoffSensitivity < 0f || pensionRate < 0f || pensionRate > 1f || float.IsNaN(marketBeta))
            {
                throw new ArgumentOutOfRangeException(nameof(stressPerHour), "Stress, volatility, layoff sensitivity and pension rate are out of range.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Category = category;
            Premium = premium;
            Requirement = requirement;
            Hiring = hiring ?? throw new ArgumentNullException(nameof(hiring));
            WeeklyHours = weeklyHours;
            PerformanceSkillId = performanceSkillId == null ? null : ContentId.Require(performanceSkillId, nameof(performanceSkillId));
            Skills = (skills ?? Enumerable.Empty<ActivitySkillEffect>()).ToList();
            StressPerHour = stressPerHour;
            IncomeVolatility = incomeVolatility;
            MarketBeta = marketBeta;
            LayoffSensitivity = layoffSensitivity;
            PensionRate = pensionRate;
            Risk = risk;
            Levels = (levels ?? throw new ArgumentNullException(nameof(levels))).ToList();
            if (Levels.Count == 0 || Levels.Select(l => l.Id).Distinct(StringComparer.Ordinal).Count() != Levels.Count)
            {
                throw new ArgumentException("A track needs at least one level and unique level ids.", nameof(levels));
            }
        }

        /// <summary>Unique id.</summary>
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Category.</summary>
        public CareerCategory Category { get; }

        /// <summary>Premium (deeper, never better paid).</summary>
        public bool Premium { get; }

        /// <summary>Condition to apply, or null.</summary>
        public ICondition Requirement { get; }

        /// <summary>Base hiring chance.</summary>
        public ChanceSpec Hiring { get; }

        /// <summary>Work hours per week.</summary>
        public int WeeklyHours { get; }

        /// <summary>Skill counted in performance, or null.</summary>
        public string PerformanceSkillId { get; }

        /// <summary>Skills practiced per work hour.</summary>
        public IReadOnlyList<ActivitySkillEffect> Skills { get; }

        /// <summary>Stress per work hour.</summary>
        public float StressPerHour { get; }

        /// <summary>Monthly income noise.</summary>
        public float IncomeVolatility { get; }

        /// <summary>Sensitivity of income to the economy.</summary>
        public float MarketBeta { get; }

        /// <summary>Multiplier of the layoff chance.</summary>
        public float LayoffSensitivity { get; }

        /// <summary>Pension share of the last salary.</summary>
        public float PensionRate { get; }

        /// <summary>Monthly risk, or null.</summary>
        public CareerRisk Risk { get; }

        /// <summary>Ladder from the entry level up.</summary>
        public IReadOnlyList<CareerLevelDefinition> Levels { get; }

        /// <summary>True when income varies month to month.</summary>
        public bool HasVariableIncome => IncomeVolatility > 0f || MarketBeta != 0f;

        /// <summary>
        /// Returns the index of a level id, or -1.
        /// </summary>
        /// <param name="levelId">Level id.</param>
        /// <returns>Index.</returns>
        public int IndexOf(string levelId) => Levels.ToList().FindIndex(l => l.Id == levelId);
    }
}
