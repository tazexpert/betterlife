using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"not","condition":{...}}.
    /// </summary>
    public sealed class NotConditionConverter : ConditionConverter<NotCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "not";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override NotCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "condition");
            var (token, childContext) = RuleJson.RequireProperty(json, "condition", context);
            return new NotCondition(registry.ParseCondition(token, childContext));
        }

        /// <inheritdoc />
        protected override void WriteTyped(NotCondition condition, JObject json, RuleRegistry registry)
        {
            json["condition"] = registry.WriteCondition(condition.Child);
        }
    }
}
