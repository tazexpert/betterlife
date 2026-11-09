using System;
using System.Collections.Generic;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// How idle hours raise temptation.
    /// </summary>
    public sealed class LazinessRules
    {
        /// <summary>
        /// Creates the value.
        /// </summary>
        /// <param name="activityIds">Activities counted as idle.</param>
        /// <param name="baselineHours">Idle hours per week that do not count.</param>
        /// <param name="perHour">Multiplier added per idle hour above the baseline.</param>
        /// <param name="max">Largest multiplier.</param>
        public LazinessRules(IReadOnlyCollection<string> activityIds, int baselineHours, float perHour, float max)
        {
            if (baselineHours < 0 || perHour < 0f || max < 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(activityIds), "Values are out of range.");
            }
            ActivityIds = activityIds ?? throw new ArgumentNullException(nameof(activityIds));
            BaselineHours = baselineHours;
            PerHour = perHour;
            Max = max;
        }

        /// <summary>Activities counted as idle.</summary>
        public IReadOnlyCollection<string> ActivityIds { get; }

        /// <summary>Idle hours per week that do not count.</summary>
        public int BaselineHours { get; }

        /// <summary>Multiplier added per idle hour above the baseline.</summary>
        public float PerHour { get; }

        /// <summary>Largest multiplier.</summary>
        public float Max { get; }
    }
}
