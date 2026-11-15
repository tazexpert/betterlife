using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Luck
{
    /// <summary>
    /// A stat's contribution to a chance: (value − 50) × weight percent.
    /// </summary>
    public sealed class ChanceStatTerm
    {
        /// <summary>Stat value that contributes nothing.</summary>
        public const float Neutral = 50f;

        /// <summary>
        /// Creates a term.
        /// </summary>
        /// <param name="statId">Stat (stored or derived).</param>
        /// <param name="weight">Percent per stat point; finite, may be negative.</param>
        public ChanceStatTerm(StatId statId, float weight)
        {
            if (float.IsNaN(weight) || float.IsInfinity(weight))
            {
                throw new ArgumentException("Weight must be finite.", nameof(weight));
            }
            StatId = statId;
            Weight = weight;
        }

        /// <summary>Stat.</summary>
        public StatId StatId { get; }

        /// <summary>Percent per stat point.</summary>
        public float Weight { get; }

        /// <summary>
        /// Returns the contribution for a stat value.
        /// </summary>
        /// <param name="value">Stat value.</param>
        /// <returns>Percent points.</returns>
        public float Contribution(float value) => (value - Neutral) * Weight;
    }
}
