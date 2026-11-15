using System;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// The fixed range of every stat value.
    /// </summary>
    public static class StatBounds
    {
        /// <summary>Lowest possible stat value.</summary>
        public const float Min = 0f;

        /// <summary>Highest possible stat value.</summary>
        public const float Max = 100f;

        /// <summary>
        /// Clamps a value into [<see cref="Min"/>, <see cref="Max"/>].
        /// </summary>
        /// <param name="value">Value to clamp.</param>
        /// <returns>The clamped value.</returns>
        public static float Clamp(float value)
        {
            return Math.Max(Min, Math.Min(Max, value));
        }

        /// <summary>
        /// Returns true when the value is finite and inside the stat range.
        /// </summary>
        /// <param name="value">Value to test.</param>
        /// <returns>True when in range.</returns>
        public static bool IsInRange(float value)
        {
            return !float.IsNaN(value) && value >= Min && value <= Max;
        }
    }
}
