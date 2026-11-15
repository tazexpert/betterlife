using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"trait.add","trait":id}.
    /// </summary>
    public sealed class AddTraitEffectConverter : EffectConverter<AddTraitEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "trait.add";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override AddTraitEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "trait");
            return new AddTraitEffect(RuleJson.RequireTrait(json, "trait", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(AddTraitEffect effect, JObject json, RuleRegistry registry)
        {
            json["trait"] = effect.TraitId;
        }
    }
}
