using BetterLife.Core.Content.Loaders;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Reads {"type":"relationship","relation":"spouse","count":{"op":"&gt;=","value":1}} or
    /// {"type":"relationship","relation":"friend","op":"&gt;=","value":60} ("relation" may be "any").
    /// </summary>
    public sealed class RelationshipConditionConverter : ConditionConverter<RelationshipCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "relationship";

        /// <summary>Relation name matching every type.</summary>
        public const string AnyRelation = "any";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override RelationshipCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "relation", "count", "op", "value");
            RelationshipType? type = ReadRelation(json, context);
            bool countMode = json["count"] != null;
            if (countMode == (json["op"] != null || json["value"] != null))
            {
                throw context.Error("needs either 'count' or 'op' + 'value'.");
            }
            if (countMode)
            {
                var (token, countContext) = RuleJson.RequireProperty(json, "count", context);
                JObject count = RuleJson.RequireObject(token, countContext);
                RuleJson.RequireOnly(count, countContext, "op", "value");
                return new RelationshipCondition(type, true, RuleJson.RequireOperator(count, countContext), RuleJson.RequireInt(count, "value", countContext));
            }
            return new RelationshipCondition(type, false, RuleJson.RequireOperator(json, context), RuleJson.RequireFloat(json, "value", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(RelationshipCondition condition, JObject json, RuleRegistry registry)
        {
            json["relation"] = condition.Type.HasValue ? RelationshipNames.NameOf(condition.Type.Value) : AnyRelation;
            if (condition.CountMode)
            {
                json["count"] = new JObject { ["op"] = Comparison.ToSymbol(condition.Operator), ["value"] = (int)condition.Value };
            }
            else
            {
                json["op"] = Comparison.ToSymbol(condition.Operator);
                json["value"] = RuleJson.Number(condition.Value);
            }
        }

        internal static RelationshipType? ReadRelation(JObject json, RuleReadContext context)
        {
            string relation = RuleJson.RequireString(json, "relation", context);
            return relation == AnyRelation ? (RelationshipType?)null : ContentReading.RequireEnum(json, "relation", context, RelationshipNames.Types);
        }
    }
}
