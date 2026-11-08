using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Domain.Traits
{
    /// <summary>
    /// Immutable definition of a trait, loaded from content/traits.json.
    /// </summary>
    public sealed class TraitDefinition : IDefinition
    {
        /// <summary>
        /// Creates a trait definition.
        /// </summary>
        /// <param name="id">Trait id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="descKey">Localization key of the description.</param>
        /// <param name="category">Category id, for example "personality".</param>
        /// <param name="exclusiveGroup">Group id; a character holds at most one trait per group. Null for none.</param>
        /// <param name="startsHidden">True when the player must discover the trait.</param>
        /// <param name="statModifiers">Stat change multipliers.</param>
        public TraitDefinition(
            string id,
            string nameKey,
            string descKey,
            string category,
            string exclusiveGroup,
            bool startsHidden,
            IEnumerable<TraitStatModifier> statModifiers)
        {
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            DescKey = ContentId.RequireLocKey(descKey, nameof(descKey));
            Category = ContentId.Require(category, nameof(category));
            ExclusiveGroup = exclusiveGroup == null ? null : ContentId.Require(exclusiveGroup, nameof(exclusiveGroup));
            StartsHidden = startsHidden;
            StatModifiers = (statModifiers ?? Enumerable.Empty<TraitStatModifier>()).ToList();
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Localization key of the description.</summary>
        public string DescKey { get; }

        /// <summary>Category id.</summary>
        public string Category { get; }

        /// <summary>Exclusive group id, or null.</summary>
        public string ExclusiveGroup { get; }

        /// <summary>True when the player must discover the trait.</summary>
        public bool StartsHidden { get; }

        /// <summary>Stat change multipliers.</summary>
        public IReadOnlyList<TraitStatModifier> StatModifiers { get; }
    }
}
