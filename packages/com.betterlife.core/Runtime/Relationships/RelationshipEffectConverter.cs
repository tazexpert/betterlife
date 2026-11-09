using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Reads {"type":"relationship","relation":"parent","amount":5 or {min,max},"target":"closest"|"all","memory":"memory.key"}
    /// ("target" defaults to closest, "memory" is optional).
    /// </summary>
    public sealed class RelationshipEffectConverter : EffectConverter<RelationshipEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "relationship";

        private const string Closest = "closest";
        private const string All = "all";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override RelationshipEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "relation", "amount", "target", "memory");
            string target = json["target"] == null ? Closest : RuleJson.RequireString(json, "target", context);
            if (target != Closest && target != All)
            {
                throw context.Property("target").Error($"'{target}' is not one of: {Closest}, {All}.");
            }
            string memory = json["memory"] == null ? null : RuleJson.RequireId(json, "memory", context);
            return new RelationshipEffect(RelationshipConditionConverter.ReadRelation(json, context), RuleJson.RequireFloatRange(json, "amount", context),
                target == All, memory);
        }

        /// <inheritdoc />
        protected override void WriteTyped(RelationshipEffect effect, JObject json, RuleRegistry registry)
        {
            json["relation"] = effect.Type.HasValue ? RelationshipNames.NameOf(effect.Type.Value) : RelationshipConditionConverter.AnyRelation;
            json["amount"] = RuleJson.Range(effect.Amount);
            json["target"] = effect.All ? All : Closest;
            if (effect.MemoryKey != null)
            {
                json["memory"] = effect.MemoryKey;
            }
        }
    }
}
