using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// Reads {"type":"education.performance","amount":5 or {min,max}}.
    /// </summary>
    public sealed class EducationPerformanceEffectConverter : EffectConverter<EducationPerformanceEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "education.performance";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override EducationPerformanceEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "amount");
            return new EducationPerformanceEffect(RuleJson.RequireFloatRange(json, "amount", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(EducationPerformanceEffect effect, JObject json, RuleRegistry registry)
        {
            json["amount"] = RuleJson.Range(effect.Amount);
        }
    }
}
