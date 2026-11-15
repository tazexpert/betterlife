using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"flag.clear","flag":id}.
    /// </summary>
    public sealed class ClearFlagEffectConverter : EffectConverter<ClearFlagEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "flag.clear";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override ClearFlagEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "flag");
            return new ClearFlagEffect(RuleJson.RequireFlag(json, "flag", context, null));
        }

        /// <inheritdoc />
        protected override void WriteTyped(ClearFlagEffect effect, JObject json, RuleRegistry registry)
        {
            json["flag"] = effect.FlagId;
        }
    }
}
