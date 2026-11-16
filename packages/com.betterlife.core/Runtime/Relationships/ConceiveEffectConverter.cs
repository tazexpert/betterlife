using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Reads <c>{ "type": "relationship.conceive" }</c>.
    /// </summary>
    public sealed class ConceiveEffectConverter : EffectConverter<ConceiveEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "relationship.conceive";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override ConceiveEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context);
            return new ConceiveEffect();
        }

        /// <inheritdoc />
        protected override void WriteTyped(ConceiveEffect effect, JObject json, RuleRegistry registry)
        {
        }
    }
}
