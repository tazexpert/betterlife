using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"skill","skill":id,"op":"&gt;=","value":40}.
    /// </summary>
    public sealed class SkillConditionConverter : ConditionConverter<SkillCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "skill";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override SkillCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "skill", "op", "value");
            return new SkillCondition(RuleJson.RequireSkill(json, "skill", context), RuleJson.RequireOperator(json, context), RuleJson.RequireFloat(json, "value", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(SkillCondition condition, JObject json, RuleRegistry registry)
        {
            json["skill"] = condition.SkillId;
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["value"] = RuleJson.Number(condition.Value);
        }
    }
}
