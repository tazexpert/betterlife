using BetterLife.Core.Rules.Conditions;
using BetterLife.Core.Rules.Effects;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"healthCondition.add","condition":id,"severity":30 or {min,max}}.
    /// </summary>
    public sealed class AddHealthConditionEffectConverter : EffectConverter<AddHealthConditionEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "healthCondition.add";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override AddHealthConditionEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "condition", "severity");
            return new AddHealthConditionEffect(RuleJson.RequireHealthCondition(json, "condition", context), RuleJson.RequireFloatRange(json, "severity", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(AddHealthConditionEffect effect, JObject json, RuleRegistry registry)
        {
            json["condition"] = effect.ConditionId;
            json["severity"] = RuleJson.Range(effect.Severity);
        }
    }
}
