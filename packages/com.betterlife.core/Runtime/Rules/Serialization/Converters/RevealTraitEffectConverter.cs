using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"trait.reveal","trait":id}.
    /// </summary>
    public sealed class RevealTraitEffectConverter : EffectConverter<RevealTraitEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "trait.reveal";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override RevealTraitEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "trait");
            return new RevealTraitEffect(RuleJson.RequireTrait(json, "trait", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(RevealTraitEffect effect, JObject json, RuleRegistry registry)
        {
            json["trait"] = effect.TraitId;
        }
    }
}
