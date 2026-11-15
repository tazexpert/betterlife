using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"year","op":"&gt;=","value":2000}.
    /// </summary>
    public sealed class YearConditionConverter : ConditionConverter<YearCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "year";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override YearCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "op", "value");
            return new YearCondition(RuleJson.RequireOperator(json, context), RuleJson.RequireInt(json, "value", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(YearCondition condition, JObject json, RuleRegistry registry)
        {
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["value"] = condition.Year;
        }
    }
}
