using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Reads {"type":"karma.good","amount":5 or {min,max}}.
    /// </summary>
    public sealed class GoodKarmaEffectConverter : EffectConverter<GoodKarmaEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "karma.good";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override GoodKarmaEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "amount");
            return new GoodKarmaEffect(RuleJson.RequireFloatRange(json, "amount", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(GoodKarmaEffect effect, JObject json, RuleRegistry registry)
        {
            json["amount"] = RuleJson.Range(effect.Amount);
        }
    }
}
