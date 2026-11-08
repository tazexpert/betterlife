using BetterLife.Core.Rules.Conditions;
using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"if","condition":{...},"then":[...],"else":[...]?}.
    /// </summary>
    public sealed class IfEffectConverter : EffectConverter<IfEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "if";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override IfEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "condition", "then", "else");
            var (conditionToken, conditionContext) = RuleJson.RequireProperty(json, "condition", context);
            var (thenToken, thenContext) = RuleJson.RequireProperty(json, "then", context);
            EffectSequence otherwise = json["else"] == null
                ? EffectSequence.Empty
                : registry.ParseEffects(json["else"], context.Property("else"));
            return new IfEffect(registry.ParseCondition(conditionToken, conditionContext), registry.ParseEffects(thenToken, thenContext), otherwise);
        }

        /// <inheritdoc />
        protected override void WriteTyped(IfEffect effect, JObject json, RuleRegistry registry)
        {
            json["condition"] = registry.WriteCondition(effect.Condition);
            json["then"] = registry.WriteEffects(effect.Then);
            if (effect.Else.Effects.Count > 0)
            {
                json["else"] = registry.WriteEffects(effect.Else);
            }
        }
    }
}
