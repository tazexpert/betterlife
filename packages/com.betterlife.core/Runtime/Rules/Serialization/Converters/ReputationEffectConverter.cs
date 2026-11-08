using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"reputation","domain":id,"amount":5 or {min,max}}.
    /// </summary>
    public sealed class ReputationEffectConverter : EffectConverter<ReputationEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "reputation";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override ReputationEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "domain", "amount");
            return new ReputationEffect(RuleJson.RequireReputationDomain(json, "domain", context), RuleJson.RequireFloatRange(json, "amount", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(ReputationEffect effect, JObject json, RuleRegistry registry)
        {
            json["domain"] = effect.DomainId;
            json["amount"] = RuleJson.Range(effect.Amount);
        }
    }
}
