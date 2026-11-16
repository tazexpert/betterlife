using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Reads <c>{ "type": "career.retire" }</c>.
    /// </summary>
    public sealed class RetireEffectConverter : EffectConverter<RetireEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "career.retire";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override RetireEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context);
            return new RetireEffect();
        }

        /// <inheritdoc />
        protected override void WriteTyped(RetireEffect effect, JObject json, RuleRegistry registry)
        {
        }
    }
}
