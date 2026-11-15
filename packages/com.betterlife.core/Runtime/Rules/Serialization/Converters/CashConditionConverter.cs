using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"cash","op":"&gt;=","amount":5000}.
    /// </summary>
    public sealed class CashConditionConverter : ConditionConverter<CashCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "cash";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override CashCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "op", "amount");
            return new CashCondition(RuleJson.RequireOperator(json, context), RuleJson.RequireDecimal(json, "amount", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(CashCondition condition, JObject json, RuleRegistry registry)
        {
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["amount"] = RuleJson.Number(condition.Amount);
        }
    }
}
