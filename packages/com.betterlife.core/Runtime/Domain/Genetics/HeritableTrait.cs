using System;

namespace BetterLife.Core.Domain.Genetics
{
    /// <summary>
    /// How common a trait is and how strongly it is passed on.
    /// </summary>
    public sealed class HeritableTrait
    {
        /// <summary>
        /// Creates the rule.
        /// </summary>
        /// <param name="traitId">Trait id.</param>
        /// <param name="prevalence">Chance (0-1) without a parent carrying it.</param>
        /// <param name="heritability">Chance (0-1) per carrying parent of passing it on.</param>
        public HeritableTrait(string traitId, float prevalence, float heritability)
        {
            if (!(prevalence >= 0f) || prevalence > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(prevalence), prevalence, "Prevalence must be within 0-1.");
            }
            if (!(heritability >= 0f) || heritability > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(heritability), heritability, "Heritability must be within 0-1.");
            }
            TraitId = ContentId.Require(traitId, nameof(traitId));
            Prevalence = prevalence;
            Heritability = heritability;
        }

        /// <summary>Trait id.</summary>
        public string TraitId { get; }

        /// <summary>Chance without a carrying parent.</summary>
        public float Prevalence { get; }

        /// <summary>Chance per carrying parent.</summary>
        public float Heritability { get; }

        /// <summary>
        /// Returns the chance that a child gets the trait.
        /// </summary>
        /// <param name="carryingParents">Number of parents with the trait.</param>
        /// <returns>Chance, 0-1.</returns>
        public float ChildChance(int carryingParents)
        {
            if (carryingParents < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(carryingParents), carryingParents, "Count must be non-negative.");
            }
            float inherited = 1f - (float)Math.Pow(1f - Heritability, carryingParents);
            return Math.Max(Prevalence, inherited);
        }
    }
}
