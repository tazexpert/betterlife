using System;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Natural weekly drift of a stat toward a resting value. Applied by the time system.
    /// </summary>
    public sealed class StatDecayRule
    {
        /// <summary>
        /// Creates a decay rule.
        /// </summary>
        /// <param name="target">Resting value the stat drifts toward (0-100).</param>
        /// <param name="ratePerWeek">Maximum movement per week; must be non-negative.</param>
        public StatDecayRule(float target, float ratePerWeek)
        {
            if (!StatBounds.IsInRange(target))
            {
                throw new ArgumentOutOfRangeException(nameof(target), target, "Target must be within stat bounds.");
            }
            if (float.IsNaN(ratePerWeek) || ratePerWeek < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(ratePerWeek), ratePerWeek, "Rate must be non-negative.");
            }

            Target = target;
            RatePerWeek = ratePerWeek;
        }

        /// <summary>Resting value.</summary>
        public float Target { get; }

        /// <summary>Maximum movement per week.</summary>
        public float RatePerWeek { get; }
    }
}
