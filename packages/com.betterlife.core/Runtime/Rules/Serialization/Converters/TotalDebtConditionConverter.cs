using BetterLife.Core.Economy;
using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"totalDebt","op":"&gt;","amount":100000,"kind":"informal"} ("kind" is optional).
    /// </summary>
    public sealed class TotalDebtConditionConverter : ConditionConverter<TotalDebtCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "totalDebt";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override TotalDebtCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "op", "amount", "kind");
            DebtKind? kind = null;
            if (json["kind"] != null)
            {
                string text = RuleJson.RequireString(json, "kind", context);
                if (!EconomyNames.DebtKinds.TryGetValue(text, out DebtKind value))
                {
                    throw context.Property("kind").Error($"'{text}' is not one of: {string.Join(", ", EconomyNames.DebtKinds.Keys)}.");
                }
                kind = value;
            }
            return new TotalDebtCondition(RuleJson.RequireOperator(json, context), RuleJson.RequireDecimal(json, "amount", context), kind);
        }

        /// <inheritdoc />
        protected override void WriteTyped(TotalDebtCondition condition, JObject json, RuleRegistry registry)
        {
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["amount"] = RuleJson.Number(condition.Amount);
            if (condition.Kind.HasValue)
            {
                json["kind"] = EconomyNames.NameOf(condition.Kind.Value);
            }
        }
    }
}
