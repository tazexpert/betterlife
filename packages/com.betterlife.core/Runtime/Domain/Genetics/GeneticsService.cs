using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Traits;
using BetterLife.Core.Randomness;

namespace BetterLife.Core.Domain.Genetics
{
    /// <summary>
    /// Rolls founder genes and inherits genes and traits from two parents.
    /// </summary>
    public sealed class GeneticsService
    {
        private readonly DefinitionCatalog<TraitDefinition> _traits;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="settings">Inheritance rules.</param>
        /// <param name="traits">Trait catalog (for exclusive groups).</param>
        public GeneticsService(GeneticsSettings settings, DefinitionCatalog<TraitDefinition> traits)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _traits = traits ?? throw new ArgumentNullException(nameof(traits));
        }

        /// <summary>Inheritance rules.</summary>
        public GeneticsSettings Settings { get; }

        /// <summary>
        /// Rolls the genes of someone without known parents (population distribution).
        /// </summary>
        /// <param name="random">Random stream.</param>
        /// <returns>The genome.</returns>
        public Genome RollFounder(IRandomProvider random)
        {
            var genome = new Genome();
            foreach (HeritableStat stat in Settings.Stats)
            {
                genome.Set(stat.StatId, random.NextGaussian(Settings.PopulationMean, Settings.PopulationSd));
            }
            return genome;
        }

        /// <summary>
        /// Rolls the traits of someone without known parents (each trait at its prevalence, respecting exclusive groups).
        /// </summary>
        /// <param name="random">Random stream.</param>
        /// <returns>Trait ids.</returns>
        public IReadOnlyList<string> RollFounderTraits(IRandomProvider random)
        {
            return Pick(random, t => t.Prevalence);
        }

        /// <summary>
        /// Inherits a child's genome: heritability × parents' average + (1 − heritability) × mean + noise.
        /// </summary>
        /// <param name="parents">Parents' genomes (at least one).</param>
        /// <param name="random">Random stream.</param>
        /// <returns>The child's genome.</returns>
        public Genome Inherit(IReadOnlyList<Genome> parents, IRandomProvider random)
        {
            if (parents == null || parents.Count == 0)
            {
                throw new ArgumentException("At least one parent is required.", nameof(parents));
            }

            var child = new Genome();
            foreach (HeritableStat stat in Settings.Stats)
            {
                float midParent = parents.Average(p => p.Get(stat.StatId) ?? Settings.PopulationMean);
                float expected = stat.Heritability * midParent + (1f - stat.Heritability) * Settings.PopulationMean;
                child.Set(stat.StatId, random.NextGaussian(expected, stat.Noise));
            }
            return child;
        }

        /// <summary>
        /// Rolls the traits a child inherits from parents' traits.
        /// </summary>
        /// <param name="parentTraits">Each parent's trait ids.</param>
        /// <param name="random">Random stream.</param>
        /// <returns>Inherited trait ids (exclusive groups respected).</returns>
        public IReadOnlyList<string> InheritTraits(IReadOnlyList<IReadOnlyList<string>> parentTraits, IRandomProvider random)
        {
            if (parentTraits == null)
            {
                throw new ArgumentNullException(nameof(parentTraits));
            }
            return Pick(random, t => t.ChildChance(parentTraits.Count(p => p.Contains(t.TraitId))));
        }

        private IReadOnlyList<string> Pick(IRandomProvider random, Func<HeritableTrait, float> chance)
        {
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var picked = new List<string>();
            var groups = new HashSet<string>(StringComparer.Ordinal);
            foreach (HeritableTrait trait in Settings.Traits)
            {
                // Always roll so the number of random values used does not depend on earlier results.
                bool hit = random.Chance(chance(trait));
                string group = _traits.Get(trait.TraitId).ExclusiveGroup;
                if (!hit || (group != null && groups.Contains(group)))
                {
                    continue;
                }
                picked.Add(trait.TraitId);
                if (group != null)
                {
                    groups.Add(group);
                }
            }
            return picked;
        }
    }
}
