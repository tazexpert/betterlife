using System;
using BetterLife.Core.Randomness;

namespace BetterLife.Core.Rules
{
    /// <summary>
    /// A fixed money amount or an inclusive range rolled each time it is used, rounded to satang.
    /// </summary>
    public readonly struct DecimalRange
    {
        /// <summary>Decimal places money is rounded to.</summary>
        public const int MoneyDecimals = 2;

        /// <summary>
        /// Creates a range.
        /// </summary>
        /// <param name="min">Lower bound.</param>
        /// <param name="max">Upper bound; at least <paramref name="min"/>.</param>
        public DecimalRange(decimal min, decimal max)
        {
            if (max < min)
            {
                throw new ArgumentOutOfRangeException(nameof(max), max, "Max must be at least min.");
            }
            Min = min;
            Max = max;
        }

        /// <summary>Lower bound.</summary>
        public decimal Min { get; }

        /// <summary>Upper bound.</summary>
        public decimal Max { get; }

        /// <summary>True when min equals max.</summary>
        public bool IsFixed => Min == Max;

        /// <summary>
        /// Creates a fixed amount.
        /// </summary>
        /// <param name="value">The amount.</param>
        /// <returns>A range with min = max.</returns>
        public static DecimalRange Fixed(decimal value) => new DecimalRange(value, value);

        /// <summary>
        /// Returns the amount: fixed values consume no randomness, ranges roll uniformly and round to satang.
        /// </summary>
        /// <param name="random">Random stream.</param>
        /// <returns>An amount in [Min, Max].</returns>
        public decimal Roll(IRandomProvider random)
        {
            if (IsFixed)
            {
                return Min;
            }
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }
            decimal rolled = Min + (decimal)random.NextFloat() * (Max - Min);
            return Math.Round(rolled, MoneyDecimals, MidpointRounding.AwayFromZero);
        }
    }
}
