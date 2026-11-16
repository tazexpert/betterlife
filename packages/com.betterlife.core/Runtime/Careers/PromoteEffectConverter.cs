using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Reads <c>{ "type": "career.promote" }</c>.
    /// </summary>
    public sealed class PromoteEffectConverter : EffectConverter<PromoteEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "career.promote";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override PromoteEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context);
            return new PromoteEffect();
        }

        /// <inheritdoc />
        protected override void WriteTyped(PromoteEffect effect, JObject json, RuleRegistry registry)
        {
        }
    }
}
