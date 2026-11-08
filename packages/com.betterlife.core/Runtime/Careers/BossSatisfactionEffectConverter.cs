using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Reads {"type":"career.boss","amount":5 or {min,max}}.
    /// </summary>
    public sealed class BossSatisfactionEffectConverter : EffectConverter<BossSatisfactionEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "career.boss";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override BossSatisfactionEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "amount");
            return new BossSatisfactionEffect(RuleJson.RequireFloatRange(json, "amount", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(BossSatisfactionEffect effect, JObject json, RuleRegistry registry)
        {
            json["amount"] = RuleJson.Range(effect.Amount);
        }
    }
}
