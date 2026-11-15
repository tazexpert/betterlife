using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"netWorth","op":"&lt;","amount":0}.
    /// </summary>
    public sealed class NetWorthConditionConverter : ConditionConverter<NetWorthCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "netWorth";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override NetWorthCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "op", "amount");
            return new NetWorthCondition(RuleJson.RequireOperator(json, context), RuleJson.RequireDecimal(json, "amount", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(NetWorthCondition condition, JObject json, RuleRegistry registry)
        {
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["amount"] = RuleJson.Number(condition.Amount);
        }
    }
}
