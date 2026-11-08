using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Reads {"type":"career.performance","amount":5 or {min,max}}.
    /// </summary>
    public sealed class CareerPerformanceEffectConverter : EffectConverter<CareerPerformanceEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "career.performance";

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override CareerPerformanceEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "amount");
            return new CareerPerformanceEffect(RuleJson.RequireFloatRange(json, "amount", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(CareerPerformanceEffect effect, JObject json, RuleRegistry registry)
        {
            json["amount"] = RuleJson.Range(effect.Amount);
        }
    }
}
