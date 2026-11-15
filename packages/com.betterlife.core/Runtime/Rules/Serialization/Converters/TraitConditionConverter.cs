using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"trait","trait":id} with optional "revealed".
    /// </summary>
    public sealed class TraitConditionConverter : ConditionConverter<TraitCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "trait";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override TraitCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "trait", "revealed");
            return new TraitCondition(RuleJson.RequireTrait(json, "trait", context), RuleJson.OptionalBool(json, "revealed", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(TraitCondition condition, JObject json, RuleRegistry registry)
        {
            json["trait"] = condition.TraitId;
            if (condition.Revealed.HasValue)
            {
                json["revealed"] = condition.Revealed.Value;
            }
        }
    }
}
