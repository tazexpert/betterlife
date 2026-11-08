using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"stat","stat":id,"amount":5 or {min,max},"source":key?}. Derived stats are rejected.
    /// </summary>
    public sealed class StatEffectConverter : EffectConverter<StatEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "stat";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override StatEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "stat", "amount", "source");
            return new StatEffect(RuleJson.RequireStat(json, "stat", context, true), RuleJson.RequireFloatRange(json, "amount", context), RuleJson.OptionalId(json, "source", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(StatEffect effect, JObject json, RuleRegistry registry)
        {
            json["stat"] = effect.StatId.Value;
            json["amount"] = RuleJson.Range(effect.Amount);
            if (effect.SourceKey != null)
            {
                json["source"] = effect.SourceKey;
            }
        }
    }
}
