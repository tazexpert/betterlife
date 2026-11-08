using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"reputation","domain":id,"op":"&gt;=","value":50}.
    /// </summary>
    public sealed class ReputationConditionConverter : ConditionConverter<ReputationCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "reputation";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override ReputationCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "domain", "op", "value");
            return new ReputationCondition(RuleJson.RequireId(json, "domain", context), RuleJson.RequireOperator(json, context), RuleJson.RequireFloat(json, "value", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(ReputationCondition condition, JObject json, RuleRegistry registry)
        {
            json["domain"] = condition.DomainId;
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["value"] = RuleJson.Number(condition.Value);
        }
    }
}
