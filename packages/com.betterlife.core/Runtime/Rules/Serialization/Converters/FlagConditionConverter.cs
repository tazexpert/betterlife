using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"flag","flag":id} or with "op" and "value" to compare the count.
    /// </summary>
    public sealed class FlagConditionConverter : ConditionConverter<FlagCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "flag";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override FlagCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "flag", "op", "value");
            string flag = RuleJson.RequireFlag(json, "flag", context, null);
            bool hasOp = json["op"] != null;
            if (hasOp != (json["value"] != null))
            {
                throw context.Error("\"op\" and \"value\" must be given together.");
            }
            return hasOp
                ? new FlagCondition(flag, RuleJson.RequireOperator(json, context), RuleJson.RequireInt(json, "value", context))
                : new FlagCondition(flag);
        }

        /// <inheritdoc />
        protected override void WriteTyped(FlagCondition condition, JObject json, RuleRegistry registry)
        {
            json["flag"] = condition.FlagId;
            if (condition.Operator.HasValue)
            {
                json["op"] = Comparison.ToSymbol(condition.Operator.Value);
                json["value"] = condition.Value;
            }
        }
    }
}
