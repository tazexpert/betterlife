using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Reads {"type":"career.fire"}.
    /// </summary>
    public sealed class FireEffectConverter : EffectConverter<FireEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "career.fire";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override FireEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context);
            return new FireEffect();
        }

        /// <inheritdoc />
        protected override void WriteTyped(FireEffect effect, JObject json, RuleRegistry registry)
        {
        }
    }
}
