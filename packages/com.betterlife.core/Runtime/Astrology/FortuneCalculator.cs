using System;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;

namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// Computes the five fortunes. Stateless: the yearly random part is derived from the character id, year and area,
    /// so fortune never changes on reload or undo and needs no saving.
    /// </summary>
    public sealed class FortuneCalculator
    {
        private const string NoiseStreamPrefix = "fortune.";

        /// <summary>
        /// Creates the calculator.
        /// </summary>
        /// <param name="tables">Astrology content.</param>
        public FortuneCalculator(AstrologyTables tables)
        {
            Tables = tables ?? throw new ArgumentNullException(nameof(tables));
        }

        /// <summary>Astrology content.</summary>
        public AstrologyTables Tables { get; }

        /// <summary>
        /// Returns the fortune of an area in the year of <paramref name="now"/>.
        /// </summary>
        /// <param name="character">Character.</param>
        /// <param name="area">Area.</param>
        /// <param name="now">Current week.</param>
        /// <returns>Fortune, 0-100.</returns>
        public float Fortune(Character character, FortuneArea area, GameDate now) => Explain(character, area, now).Value;

        /// <summary>
        /// Explains the fortune of an area in the year of <paramref name="now"/>.
        /// </summary>
        /// <param name="character">Character.</param>
        /// <param name="area">Area.</param>
        /// <param name="now">Current week.</param>
        /// <returns>The breakdown.</returns>
        public FortuneBreakdown Explain(Character character, FortuneArea area, GameDate now)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }

            AstrologyProfile profile = Tables.ProfileFor(character.Birth);
            int year = now.Year;
            float zodiac = profile.Zodiac.Modifiers.For(area);
            float day = profile.BirthDay.Modifiers.For(area);
            float element = profile.Element.Modifiers.For(area);
            float personalPhase = profile.ZodiacIndex * Tables.Cycle.PeriodYears / AstrologyTables.CycleLength;
            float cycle = Tables.Cycle.PointsFor(year, area, personalPhase);
            float yearly = YearlyNoise(character.Id, year, area);
            float value = StatBounds.Clamp(Tables.BaseFortune + zodiac + day + element + cycle + yearly);
            return new FortuneBreakdown(area, year, Tables.BaseFortune, zodiac, day, element, cycle, yearly, value);
        }

        private float YearlyNoise(CharacterId id, int year, FortuneArea area)
        {
            if (Tables.YearlyNoise == 0f)
            {
                return 0f;
            }
            ulong seed = StableHash.Fnv1a64(id.Value);
            IRandomProvider random = Pcg32.CreateDerived(seed, NoiseStreamPrefix + year + "." + FortuneAreas.ToName(area));
            return (random.NextFloat() * 2f - 1f) * Tables.YearlyNoise;
        }
    }
}
