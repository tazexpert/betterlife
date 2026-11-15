using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Domain.Genetics
{
    /// <summary>
    /// Inheritance rules from content/genetics.json.
    /// </summary>
    public sealed class GeneticsSettings
    {
        /// <summary>
        /// Creates settings.
        /// </summary>
        /// <param name="populationMean">Population mean of heritable stats (0-100).</param>
        /// <param name="populationSd">Population standard deviation; non-negative.</param>
        /// <param name="stats">Heritable stats (unique).</param>
        /// <param name="traits">Heritable traits (unique).</param>
        public GeneticsSettings(float populationMean, float populationSd, IEnumerable<HeritableStat> stats, IEnumerable<HeritableTrait> traits)
        {
            if (!StatBounds.IsInRange(populationMean))
            {
                throw new ArgumentOutOfRangeException(nameof(populationMean), populationMean, "Mean must be within 0-100.");
            }
            if (!(populationSd >= 0f) || float.IsInfinity(populationSd))
            {
                throw new ArgumentOutOfRangeException(nameof(populationSd), populationSd, "Deviation must be non-negative.");
            }
            PopulationMean = populationMean;
            PopulationSd = populationSd;
            Stats = (stats ?? Enumerable.Empty<HeritableStat>()).ToList();
            Traits = (traits ?? Enumerable.Empty<HeritableTrait>()).ToList();
            if (Stats.Select(s => s.StatId).Distinct().Count() != Stats.Count)
            {
                throw new ArgumentException("Each stat may appear once.", nameof(stats));
            }
            if (Traits.Select(t => t.TraitId).Distinct(StringComparer.Ordinal).Count() != Traits.Count)
            {
                throw new ArgumentException("Each trait may appear once.", nameof(traits));
            }
        }

        /// <summary>Population mean.</summary>
        public float PopulationMean { get; }

        /// <summary>Population standard deviation.</summary>
        public float PopulationSd { get; }

        /// <summary>Heritable stats.</summary>
        public IReadOnlyList<HeritableStat> Stats { get; }

        /// <summary>Heritable traits.</summary>
        public IReadOnlyList<HeritableTrait> Traits { get; }
    }
}
