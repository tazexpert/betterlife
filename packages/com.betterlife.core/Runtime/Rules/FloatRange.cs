using System;
using BetterLife.Core.Randomness;

namespace BetterLife.Core.Rules
{
    /// <summary>
    /// A fixed number or an inclusive range rolled each time it is used.
    /// </summary>
    public readonly struct FloatRange
    {
        /// <summary>
        /// Creates a range.
        /// </summary>
        /// <param name="min">Lower bound; finite.</param>
        /// <param name="max">Upper bound; finite and at least <paramref name="min"/>.</param>
        public FloatRange(float min, float max)
        {
            if (!IsFinite(min) || !IsFinite(max) || max < min)
            {
                throw new ArgumentOutOfRangeException(nameof(max), max, "Range bounds must be finite with max >= min.");
            }
            Min = min;
            Max = max;
        }

        /// <summary>Lower bound.</summary>
        public float Min { get; }

        /// <summary>Upper bound.</summary>
        public float Max { get; }

        /// <summary>True when min equals max.</summary>
        public bool IsFixed => Min == Max;

        /// <summary>
        /// Creates a fixed value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>A range with min = max.</returns>
        public static FloatRange Fixed(float value) => new FloatRange(value, value);

        /// <summary>
        /// Returns the value: fixed values consume no randomness, ranges roll uniformly.
        /// </summary>
        /// <param name="random">Random stream.</param>
        /// <returns>A value in [Min, Max].</returns>
        public float Roll(IRandomProvider random)
        {
            if (IsFixed)
            {
                return Min;
            }
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }
            return Min + random.NextFloat() * (Max - Min);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
