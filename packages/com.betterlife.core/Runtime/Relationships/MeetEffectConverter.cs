using System.Collections.Generic;
using BetterLife.Core.Content.Loaders;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Reads <c>{ "type": "meet", "relation": "friend", "chance": 0.15 }</c> (relation: friend, colleague, contact or partner).
    /// </summary>
    public sealed class MeetEffectConverter : EffectConverter<MeetEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "meet";

        private static readonly IReadOnlyDictionary<string, RelationshipType> Relations = new Dictionary<string, RelationshipType>
        {
            ["friend"] = RelationshipType.Friend,
            ["colleague"] = RelationshipType.Colleague,
            ["contact"] = RelationshipType.Contact,
            ["partner"] = RelationshipType.Partner,
        };

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override MeetEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "relation", "chance");
            float chance = RuleJson.RequireFloat(json, "chance", context);
            if (!(chance > 0f) || chance > 1f)
            {
                throw context.Property("chance").Error("chance must be within (0, 1].");
            }
            return new MeetEffect(ContentReading.RequireEnum(json, "relation", context, Relations), chance);
        }

        /// <inheritdoc />
        protected override void WriteTyped(MeetEffect effect, JObject json, RuleRegistry registry)
        {
            json["relation"] = RelationshipNames.NameOf(effect.Type);
            json["chance"] = RuleJson.Number(effect.Chance);
        }
    }
}
