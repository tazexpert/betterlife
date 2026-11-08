using System;

namespace BetterLife.Core.Domain.Family
{
    /// <summary>
    /// A chance (0-1) of gaining a trait.
    /// </summary>
    public sealed class TraitChance
    {
        /// <summary>
        /// Creates a trait chance.
        /// </summary>
        /// <param name="traitId">Trait id.</param>
        /// <param name="chance">Probability, 0-1.</param>
        public TraitChance(string traitId, float chance)
        {
            if (!(chance >= 0f) || chance > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(chance), chance, "Chance must be within 0-1.");
            }
            TraitId = ContentId.Require(traitId, nameof(traitId));
            Chance = chance;
        }

        /// <summary>Trait id.</summary>
        public string TraitId { get; }

        /// <summary>Probability.</summary>
        public float Chance { get; }
    }
}
