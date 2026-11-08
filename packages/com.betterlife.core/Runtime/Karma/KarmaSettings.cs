using System;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Karma tuning from content/luck.json.
    /// </summary>
    public sealed class KarmaSettings
    {
        /// <summary>
        /// Creates settings.
        /// </summary>
        /// <param name="goodWeight">Chance points per point of good karma; non-negative.</param>
        /// <param name="badWeight">Chance points lost per point of bad karma; non-negative.</param>
        /// <param name="cap">Maximum absolute chance points from karma; non-negative.</param>
        /// <param name="debtRatio">Share of each bad deed that becomes debt, 0-1.</param>
        /// <param name="debtDelayMinWeeks">Earliest weeks until a debt is due; positive.</param>
        /// <param name="debtDelayMaxWeeks">Latest weeks until a debt is due; at least the minimum.</param>
        /// <param name="meritOffsetRatio">Share of merit that pays down debt, 0-1.</param>
        public KarmaSettings(float goodWeight, float badWeight, float cap, float debtRatio, int debtDelayMinWeeks, int debtDelayMaxWeeks, float meritOffsetRatio)
        {
            RequireNonNegative(goodWeight, nameof(goodWeight));
            RequireNonNegative(badWeight, nameof(badWeight));
            RequireNonNegative(cap, nameof(cap));
            RequireRatio(debtRatio, nameof(debtRatio));
            RequireRatio(meritOffsetRatio, nameof(meritOffsetRatio));
            if (debtDelayMinWeeks <= 0 || debtDelayMaxWeeks < debtDelayMinWeeks)
            {
                throw new ArgumentOutOfRangeException(nameof(debtDelayMaxWeeks), debtDelayMaxWeeks, "Delays must satisfy 0 < min <= max.");
            }

            GoodWeight = goodWeight;
            BadWeight = badWeight;
            Cap = cap;
            DebtRatio = debtRatio;
            DebtDelayMinWeeks = debtDelayMinWeeks;
            DebtDelayMaxWeeks = debtDelayMaxWeeks;
            MeritOffsetRatio = meritOffsetRatio;
        }

        /// <summary>Chance points per point of good karma.</summary>
        public float GoodWeight { get; }

        /// <summary>Chance points lost per point of bad karma.</summary>
        public float BadWeight { get; }

        /// <summary>Maximum absolute chance points from karma.</summary>
        public float Cap { get; }

        /// <summary>Share of each bad deed that becomes debt.</summary>
        public float DebtRatio { get; }

        /// <summary>Earliest weeks until a debt is due.</summary>
        public int DebtDelayMinWeeks { get; }

        /// <summary>Latest weeks until a debt is due.</summary>
        public int DebtDelayMaxWeeks { get; }

        /// <summary>Share of merit that pays down debt.</summary>
        public float MeritOffsetRatio { get; }

        private static void RequireNonNegative(float value, string name)
        {
            if (!(value >= 0f) || float.IsInfinity(value))
            {
                throw new ArgumentOutOfRangeException(name, value, "Value must be non-negative.");
            }
        }

        private static void RequireRatio(float value, string name)
        {
            if (!(value >= 0f) || value > 1f)
            {
                throw new ArgumentOutOfRangeException(name, value, "Ratio must be within 0-1.");
            }
        }
    }
}
