using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Genetics;

namespace BetterLife.Core.Domain.Family
{
    /// <summary>
    /// A lightweight parent: identity, personality, genes and traits. The NPC task turns parents into full NPCs.
    /// </summary>
    public sealed class ParentRecord
    {
        /// <summary>
        /// Creates a parent.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="sex">Biological sex.</param>
        /// <param name="birthYear">Birth year.</param>
        /// <param name="personalityId">Parenting personality id.</param>
        /// <param name="genome">Genes.</param>
        /// <param name="traitIds">Traits.</param>
        public ParentRecord(PersonName name, BiologicalSex sex, int birthYear, string personalityId, Genome genome, IEnumerable<string> traitIds)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Sex = sex;
            BirthYear = birthYear;
            PersonalityId = ContentId.Require(personalityId, nameof(personalityId));
            Genome = genome ?? throw new ArgumentNullException(nameof(genome));
            TraitIds = (traitIds ?? Enumerable.Empty<string>()).ToList();
        }

        /// <summary>Name.</summary>
        public PersonName Name { get; }

        /// <summary>Biological sex.</summary>
        public BiologicalSex Sex { get; }

        /// <summary>Birth year.</summary>
        public int BirthYear { get; }

        /// <summary>Parenting personality id.</summary>
        public string PersonalityId { get; }

        /// <summary>Genes.</summary>
        public Genome Genome { get; }

        /// <summary>Traits.</summary>
        public IReadOnlyList<string> TraitIds { get; }
    }
}
