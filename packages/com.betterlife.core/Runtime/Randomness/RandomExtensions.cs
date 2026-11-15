using System;

namespace BetterLife.Core.Randomness
{
    /// <summary>
    /// Distribution helpers built on <see cref="IRandomProvider"/>.
    /// </summary>
    public static class RandomExtensions
    {
        private const double TwoPi = 2.0 * Math.PI;
        private const double MinUniform = 1e-12;

        /// <summary>
        /// Returns a normally distributed value (Box-Muller; consumes two values).
        /// </summary>
        /// <param name="random">Random stream.</param>
        /// <param name="mean">Mean.</param>
        /// <param name="standardDeviation">Standard deviation; non-negative.</param>
        /// <returns>A sample.</returns>
        public static float NextGaussian(this IRandomProvider random, float mean, float standardDeviation)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }
            if (!(standardDeviation >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(standardDeviation), standardDeviation, "Deviation must be non-negative.");
            }
            double u1 = Math.Max(MinUniform, 1.0 - random.NextFloat());
            double u2 = random.NextFloat();
            double z = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(TwoPi * u2);
            return (float)(mean + z * standardDeviation);
        }

        /// <summary>
        /// Picks an item by weight; returns default when every weight is zero (consumes one value when picking).
        /// </summary>
        /// <typeparam name="T">Item type.</typeparam>
        /// <param name="random">Random stream.</param>
        /// <param name="items">Items in a stable order.</param>
        /// <param name="weight">Non-negative weight of an item.</param>
        /// <returns>The picked item or default.</returns>
        public static T PickWeighted<T>(this IRandomProvider random, System.Collections.Generic.IReadOnlyList<T> items, Func<T, float> weight)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            float total = 0f;
            foreach (T item in items)
            {
                total += Math.Max(0f, weight(item));
            }
            if (total <= 0f)
            {
                return default;
            }

            float roll = random.NextFloat() * total;
            T last = default;
            foreach (T item in items)
            {
                float w = Math.Max(0f, weight(item));
                if (w <= 0f)
                {
                    continue;
                }
                last = item;
                roll -= w;
                if (roll < 0f)
                {
                    return item;
                }
            }
            return last;
        }
    }
}
