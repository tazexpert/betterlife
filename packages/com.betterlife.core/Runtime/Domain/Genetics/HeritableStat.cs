using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Domain.Genetics
{
    /// <summary>
    /// How strongly a stat is inherited.
    /// </summary>
    public sealed class HeritableStat
    {
        /// <summary>
        /// Creates the rule.
        /// </summary>
        /// <param name="statId">Stored stat.</param>
        /// <param name="heritability">Weight of the parents' average versus the population mean, 0-1.</param>
        /// <param name="noise">Standard deviation of random variation; non-negative.</param>
        public HeritableStat(StatId statId, float heritability, float noise)
        {
            if (!(heritability >= 0f) || heritability > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(heritability), heritability, "Heritability must be within 0-1.");
            }
            if (!(noise >= 0f) || float.IsInfinity(noise))
            {
                throw new ArgumentOutOfRangeException(nameof(noise), noise, "Noise must be non-negative.");
            }
            StatId = statId;
            Heritability = heritability;
            Noise = noise;
        }

        /// <summary>Stored stat.</summary>
        public StatId StatId { get; }

        /// <summary>Heritability, 0-1.</summary>
        public float Heritability { get; }

        /// <summary>Standard deviation of variation.</summary>
        public float Noise { get; }
    }
}
