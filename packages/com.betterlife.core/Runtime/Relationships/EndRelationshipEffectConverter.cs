using System.Collections.Generic;
using BetterLife.Core.Content.Loaders;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Reads <c>{ "type": "relationship.end", "relation": "spouse" }</c> (relation: partner or spouse).
    /// </summary>
    public sealed class EndRelationshipEffectConverter : EffectConverter<EndRelationshipEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "relationship.end";

        private static readonly IReadOnlyDictionary<string, RelationshipType> Relations = new Dictionary<string, RelationshipType>
        {
            ["partner"] = RelationshipType.Partner,
            ["spouse"] = RelationshipType.Spouse,
        };

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override EndRelationshipEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "relation");
            return new EndRelationshipEffect(ContentReading.RequireEnum(json, "relation", context, Relations));
        }

        /// <inheritdoc />
        protected override void WriteTyped(EndRelationshipEffect effect, JObject json, RuleRegistry registry)
        {
            json["relation"] = RelationshipNames.NameOf(effect.Type);
        }
    }
}
