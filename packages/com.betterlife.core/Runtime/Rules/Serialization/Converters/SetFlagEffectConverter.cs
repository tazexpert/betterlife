using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;
using BetterLife.Core.Domain.Flags;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"flag.set","flag":boolFlagId,"expiryWeeks":26?}.
    /// </summary>
    public sealed class SetFlagEffectConverter : EffectConverter<SetFlagEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "flag.set";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override SetFlagEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "flag", "expiryWeeks");
            return new SetFlagEffect(RuleJson.RequireFlag(json, "flag", context, FlagKind.Bool), RuleJson.OptionalInt(json, "expiryWeeks", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(SetFlagEffect effect, JObject json, RuleRegistry registry)
        {
            json["flag"] = effect.FlagId;
            if (effect.ExpiryWeeks.HasValue)
            {
                json["expiryWeeks"] = effect.ExpiryWeeks.Value;
            }
        }
    }
}
