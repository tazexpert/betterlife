using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// JSON names and groups of relationship types.
    /// </summary>
    public static class RelationshipNames
    {
        /// <summary>Types by JSON name.</summary>
        public static readonly IReadOnlyDictionary<string, RelationshipType> Types = new Dictionary<string, RelationshipType>
        {
            ["parent"] = RelationshipType.Parent,
            ["sibling"] = RelationshipType.Sibling,
            ["child"] = RelationshipType.Child,
            ["partner"] = RelationshipType.Partner,
            ["spouse"] = RelationshipType.Spouse,
            ["ex"] = RelationshipType.Ex,
            ["friend"] = RelationshipType.Friend,
            ["colleague"] = RelationshipType.Colleague,
            ["contact"] = RelationshipType.Contact,
        };

        /// <summary>
        /// Returns the JSON name of a type.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>The name.</returns>
        public static string NameOf(RelationshipType type) => Types.First(p => p.Value == type).Key;

        /// <summary>
        /// Returns true for blood relatives (parents, siblings, children).
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>True for family.</returns>
        public static bool IsFamily(RelationshipType type) =>
            type == RelationshipType.Parent || type == RelationshipType.Sibling || type == RelationshipType.Child;

        /// <summary>
        /// Returns true for a current partner or spouse.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <returns>True for romance.</returns>
        public static bool IsRomantic(RelationshipType type) => type == RelationshipType.Partner || type == RelationshipType.Spouse;
    }
}
