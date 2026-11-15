using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Global relationship rules from content/relationship_settings.json.
    /// </summary>
    public sealed class RelationshipSettings
    {
        /// <summary>
        /// Creates the settings; every relationship type needs its rules.
        /// </summary>
        /// <param name="socialActivityId">Activity whose hours feed relationships.</param>
        /// <param name="maxRelationships">Most non-family relationships kept.</param>
        /// <param name="types">Per-type rules.</param>
        /// <param name="attention">Attention rules.</param>
        /// <param name="family">Family rules.</param>
        /// <param name="meeting">Meeting rules.</param>
        /// <param name="compatibility">Compatibility rules.</param>
        /// <param name="romance">Romance rules.</param>
        /// <param name="children">Child rules.</param>
        /// <param name="stats">Stat rules.</param>
        /// <param name="network">Network rules.</param>
        public RelationshipSettings(string socialActivityId, int maxRelationships, IEnumerable<RelationshipTypeRules> types, AttentionRules attention,
            FamilyRules family, MeetingRules meeting, CompatibilityRules compatibility, RomanceRules romance, ChildRules children,
            RelationshipStatRules stats, NetworkRules network)
        {
            if (maxRelationships < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRelationships), maxRelationships, "At least one relationship must be allowed.");
            }
            SocialActivityId = ContentId.Require(socialActivityId, nameof(socialActivityId));
            MaxRelationships = maxRelationships;
            Types = (types ?? throw new ArgumentNullException(nameof(types))).ToDictionary(t => t.Type);
            RelationshipType missing = Enum.GetValues(typeof(RelationshipType)).Cast<RelationshipType>().FirstOrDefault(t => !Types.ContainsKey(t));
            if (Types.Count != Enum.GetValues(typeof(RelationshipType)).Length)
            {
                throw new ArgumentException($"Rules for '{RelationshipNames.NameOf(missing)}' are missing.", nameof(types));
            }
            Attention = attention ?? throw new ArgumentNullException(nameof(attention));
            Family = family ?? throw new ArgumentNullException(nameof(family));
            Meeting = meeting ?? throw new ArgumentNullException(nameof(meeting));
            Compatibility = compatibility ?? throw new ArgumentNullException(nameof(compatibility));
            Romance = romance ?? throw new ArgumentNullException(nameof(romance));
            Children = children ?? throw new ArgumentNullException(nameof(children));
            Stats = stats ?? throw new ArgumentNullException(nameof(stats));
            Network = network ?? throw new ArgumentNullException(nameof(network));
        }

        /// <summary>Activity whose hours feed relationships.</summary>
        public string SocialActivityId { get; }

        /// <summary>Most non-family relationships kept.</summary>
        public int MaxRelationships { get; }

        /// <summary>Per-type rules.</summary>
        public IReadOnlyDictionary<RelationshipType, RelationshipTypeRules> Types { get; }

        /// <summary>Attention rules.</summary>
        public AttentionRules Attention { get; }

        /// <summary>Family rules.</summary>
        public FamilyRules Family { get; }

        /// <summary>Meeting rules.</summary>
        public MeetingRules Meeting { get; }

        /// <summary>Compatibility rules.</summary>
        public CompatibilityRules Compatibility { get; }

        /// <summary>Romance rules.</summary>
        public RomanceRules Romance { get; }

        /// <summary>Child rules.</summary>
        public ChildRules Children { get; }

        /// <summary>Stat rules.</summary>
        public RelationshipStatRules Stats { get; }

        /// <summary>Network rules.</summary>
        public NetworkRules Network { get; }
    }
}
