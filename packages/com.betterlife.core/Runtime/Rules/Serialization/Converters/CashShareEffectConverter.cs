using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads <c>{ "type": "cash.share", "share": 0.2 }</c> (lose 20% of the cash on hand).
    /// </summary>
    public sealed class CashShareEffectConverter : EffectConverter<CashShareEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "cash.share";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override CashShareEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "share");
            decimal share = RuleJson.RequireDecimal(json, "share", context);
            if (!(share > 0m) || share > 1m)
            {
                throw context.Property("share").Error("share must be within (0, 1].");
            }
            return new CashShareEffect(share);
        }

        /// <inheritdoc />
        protected override void WriteTyped(CashShareEffect effect, JObject json, RuleRegistry registry)
        {
            json["share"] = RuleJson.Number(effect.Share);
        }
    }
}
