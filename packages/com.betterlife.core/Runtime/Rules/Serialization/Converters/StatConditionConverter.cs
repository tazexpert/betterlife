using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"stat","stat":id,"op":"&gt;=","value":70}.
    /// </summary>
    public sealed class StatConditionConverter : ConditionConverter<StatCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "stat";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override StatCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "stat", "op", "value");
            return new StatCondition(RuleJson.RequireStat(json, "stat", context, false), RuleJson.RequireOperator(json, context), RuleJson.RequireFloat(json, "value", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(StatCondition condition, JObject json, RuleRegistry registry)
        {
            json["stat"] = condition.StatId.Value;
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["value"] = RuleJson.Number(condition.Value);
        }
    }
}
