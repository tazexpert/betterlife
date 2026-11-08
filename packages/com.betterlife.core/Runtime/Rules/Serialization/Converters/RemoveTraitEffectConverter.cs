using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"trait.remove","trait":id}.
    /// </summary>
    public sealed class RemoveTraitEffectConverter : EffectConverter<RemoveTraitEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "trait.remove";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override RemoveTraitEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "trait");
            return new RemoveTraitEffect(RuleJson.RequireTrait(json, "trait", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(RemoveTraitEffect effect, JObject json, RuleRegistry registry)
        {
            json["trait"] = effect.TraitId;
        }
    }
}
