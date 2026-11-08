using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"healthCondition","condition":id}.
    /// </summary>
    public sealed class HealthConditionConditionConverter : ConditionConverter<HealthConditionCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "healthCondition";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override HealthConditionCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "condition");
            return new HealthConditionCondition(RuleJson.RequireId(json, "condition", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(HealthConditionCondition condition, JObject json, RuleRegistry registry)
        {
            json["condition"] = condition.ConditionId;
        }
    }
}
