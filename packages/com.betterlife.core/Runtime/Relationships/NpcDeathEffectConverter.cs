using BetterLife.Core.Content.Loaders;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Reads <c>{ "type": "relationship.death", "relation": "friend" }</c>.
    /// </summary>
    public sealed class NpcDeathEffectConverter : EffectConverter<NpcDeathEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "relationship.death";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override NpcDeathEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "relation");
            return new NpcDeathEffect(ContentReading.RequireEnum(json, "relation", context, RelationshipNames.Types));
        }

        /// <inheritdoc />
        protected override void WriteTyped(NpcDeathEffect effect, JObject json, RuleRegistry registry)
        {
            json["relation"] = RelationshipNames.NameOf(effect.Type);
        }
    }
}
