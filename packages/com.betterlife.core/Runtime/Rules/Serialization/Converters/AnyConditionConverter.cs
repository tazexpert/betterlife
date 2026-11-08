using System.Linq;
using BetterLife.Core.Rules.Conditions;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Rules.Serialization.Converters
{
    /// <summary>
    /// Reads {"type":"any","conditions":[...]}.
    /// </summary>
    public sealed class AnyConditionConverter : ConditionConverter<AnyCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "any";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override AnyCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "conditions");
            var (array, arrayContext) = RuleJson.RequireArrayProperty(json, "conditions", context);
            return new AnyCondition(array.Select((item, i) => registry.ParseCondition(item, arrayContext.Index(i))).ToList());
        }

        /// <inheritdoc />
        protected override void WriteTyped(AnyCondition condition, JObject json, RuleRegistry registry)
        {
            json["conditions"] = new JArray(condition.Children.Select(registry.WriteCondition));
        }
    }
}
