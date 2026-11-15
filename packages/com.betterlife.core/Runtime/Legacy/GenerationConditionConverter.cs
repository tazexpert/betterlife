using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Legacy
{
    /// <summary>
    /// Reads <c>{ "type": "generation", "op": ">=", "value": 3 }</c>.
    /// </summary>
    public sealed class GenerationConditionConverter : ConditionConverter<GenerationCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "generation";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override GenerationCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "op", "value");
            int generation = RuleJson.RequireInt(json, "value", context);
            if (generation < 1)
            {
                throw context.Property("value").Error("generation starts at 1.");
            }
            return new GenerationCondition(RuleJson.RequireOperator(json, context), generation);
        }

        /// <inheritdoc />
        protected override void WriteTyped(GenerationCondition condition, JObject json, RuleRegistry registry)
        {
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["value"] = condition.Generation;
        }
    }
}
