using System;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Luck;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Promotion review rules.
    /// </summary>
    public sealed class PromotionRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="chance">Base chance spec.</param>
        /// <param name="perPointAbove">Chance points per performance point above the level's threshold.</param>
        /// <param name="minBoss">Boss satisfaction needed to be considered.</param>
        /// <param name="reviewEveryMonths">Months between reviews after the first.</param>
        public PromotionRules(ChanceSpec chance, float perPointAbove, float minBoss, int reviewEveryMonths)
        {
            if (perPointAbove < 0f || !StatBounds.IsInRange(minBoss) || reviewEveryMonths < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(perPointAbove), "Promotion values are out of range.");
            }
            Chance = chance ?? throw new ArgumentNullException(nameof(chance));
            PerPointAbove = perPointAbove;
            MinBoss = minBoss;
            ReviewEveryMonths = reviewEveryMonths;
        }

        /// <summary>Base chance spec.</summary>
        public ChanceSpec Chance { get; }

        /// <summary>Chance points per performance point above the threshold.</summary>
        public float PerPointAbove { get; }

        /// <summary>Boss satisfaction needed.</summary>
        public float MinBoss { get; }

        /// <summary>Months between reviews.</summary>
        public int ReviewEveryMonths { get; }
    }
}
