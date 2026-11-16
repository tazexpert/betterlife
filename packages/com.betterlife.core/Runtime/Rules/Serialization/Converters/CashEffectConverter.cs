using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"cash","amount":-5000 or {min,max}, "familyPays": true (optional, medical bills)}.
    /// </summary>
    public sealed class CashEffectConverter : EffectConverter<CashEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "cash";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override CashEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "amount", "familyPays");
            return new CashEffect(RuleJson.RequireDecimalRange(json, "amount", context), RuleJson.OptionalBool(json, "familyPays", context) ?? false);
        }

        /// <inheritdoc />
        protected override void WriteTyped(CashEffect effect, JObject json, RuleRegistry registry)
        {
            json["amount"] = RuleJson.Range(effect.Amount);
            if (effect.FamilyPays)
            {
                json["familyPays"] = true;
            }
        }
    }
}
