using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Domain.Family
{
    /// <summary>
    /// A parenting personality, loaded from content/parent_personalities.json.
    /// </summary>
    public sealed class ParentPersonalityDefinition : IDefinition
    {
        /// <summary>
        /// Creates a personality.
        /// </summary>
        /// <param name="id">Personality id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="weight">Random selection weight; non-negative.</param>
        /// <param name="statModifiers">Stat changes to the child at birth.</param>
        /// <param name="traitChances">Chances that the child gains traits.</param>
        public ParentPersonalityDefinition(string id, string nameKey, float weight, IEnumerable<StatAmount> statModifiers, IEnumerable<TraitChance> traitChances)
        {
            if (!(weight >= 0f) || float.IsInfinity(weight))
            {
                throw new ArgumentOutOfRangeException(nameof(weight), weight, "Weight must be non-negative.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Weight = weight;
            StatModifiers = (statModifiers ?? Enumerable.Empty<StatAmount>()).ToList();
            TraitChances = (traitChances ?? Enumerable.Empty<TraitChance>()).ToList();
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name (also the source key of its stat changes).</summary>
        public string NameKey { get; }

        /// <summary>Random selection weight.</summary>
        public float Weight { get; }

        /// <summary>Stat changes to the child at birth.</summary>
        public IReadOnlyList<StatAmount> StatModifiers { get; }

        /// <summary>Chances that the child gains traits.</summary>
        public IReadOnlyList<TraitChance> TraitChances { get; }
    }
}
