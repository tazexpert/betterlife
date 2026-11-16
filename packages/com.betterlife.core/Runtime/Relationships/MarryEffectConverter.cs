using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Reads <c>{ "type": "relationship.marry" }</c>.
    /// </summary>
    public sealed class MarryEffectConverter : EffectConverter<MarryEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "relationship.marry";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override MarryEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context);
            return new MarryEffect();
        }

        /// <inheritdoc />
        protected override void WriteTyped(MarryEffect effect, JObject json, RuleRegistry registry)
        {
        }
    }
}
