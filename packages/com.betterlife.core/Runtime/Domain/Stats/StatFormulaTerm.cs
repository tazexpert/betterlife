using System;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// One weighted input of a <see cref="WeightedStatFormula"/>.
    /// </summary>
    public sealed class StatFormulaTerm
    {
        /// <summary>
        /// Creates a term.
        /// </summary>
        /// <param name="source">Input stat.</param>
        /// <param name="weight">Positive weight.</param>
        /// <param name="invert">True to use (Max - value), for lower-is-better inputs.</param>
        public StatFormulaTerm(StatId source, float weight, bool invert = false)
        {
            if (float.IsNaN(weight) || weight <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(weight), weight, "Weight must be positive.");
            }

            Source = source;
            Weight = weight;
            Invert = invert;
        }

        /// <summary>Input stat.</summary>
        public StatId Source { get; }

        /// <summary>Weight.</summary>
        public float Weight { get; }

        /// <summary>Whether the input is inverted.</summary>
        public bool Invert { get; }

        /// <summary>
        /// Returns the points this term contributes for an input value.
        /// </summary>
        /// <param name="inputValue">Current input value.</param>
        /// <returns>Weighted contribution.</returns>
        public float Contribution(float inputValue)
        {
            float effective = Invert ? StatBounds.Max - inputValue : inputValue;
            return effective * Weight;
        }
    }
}
