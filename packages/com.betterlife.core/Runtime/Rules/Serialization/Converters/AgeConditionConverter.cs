using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"age","op":"&lt;","value":25}.
    /// </summary>
    public sealed class AgeConditionConverter : ConditionConverter<AgeCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "age";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override AgeCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "op", "value");
            return new AgeCondition(RuleJson.RequireOperator(json, context), RuleJson.RequireInt(json, "value", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(AgeCondition condition, JObject json, RuleRegistry registry)
        {
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["value"] = condition.Years;
        }
    }
}
