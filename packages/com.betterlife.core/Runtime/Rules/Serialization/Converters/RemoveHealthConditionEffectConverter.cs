using BetterLife.Core.Rules.Conditions;
using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"healthCondition.remove","condition":id}.
    /// </summary>
    public sealed class RemoveHealthConditionEffectConverter : EffectConverter<RemoveHealthConditionEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "healthCondition.remove";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override RemoveHealthConditionEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "condition");
            return new RemoveHealthConditionEffect(RuleJson.RequireHealthCondition(json, "condition", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(RemoveHealthConditionEffect effect, JObject json, RuleRegistry registry)
        {
            json["condition"] = effect.ConditionId;
        }
    }
}
