using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;
using BetterLife.Core.Domain.Flags;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"flag.increment","flag":counterFlagId,"amount":1?,"expiryWeeks":26?}.
    /// </summary>
    public sealed class IncrementFlagEffectConverter : EffectConverter<IncrementFlagEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "flag.increment";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override IncrementFlagEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "flag", "amount", "expiryWeeks");
            return new IncrementFlagEffect(
                RuleJson.RequireFlag(json, "flag", context, FlagKind.Counter),
                RuleJson.OptionalInt(json, "amount", context) ?? 1,
                RuleJson.OptionalInt(json, "expiryWeeks", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(IncrementFlagEffect effect, JObject json, RuleRegistry registry)
        {
            json["flag"] = effect.FlagId;
            json["amount"] = effect.Amount;
            if (effect.ExpiryWeeks.HasValue)
            {
                json["expiryWeeks"] = effect.ExpiryWeeks.Value;
            }
        }
    }
}
