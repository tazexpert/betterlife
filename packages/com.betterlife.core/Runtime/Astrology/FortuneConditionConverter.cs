using System;
using BetterLife.Core.Content.Loaders;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// Reads {"type":"fortune","area":"career","op":"&gt;=","value":60}. Registered by the astrology loader.
    /// </summary>
    public sealed class FortuneConditionConverter : ConditionConverter<FortuneCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "fortune";

        private readonly FortuneCalculator _fortune;

        /// <summary>
        /// Creates the converter.
        /// </summary>
        /// <param name="fortune">Fortune calculator injected into parsed conditions.</param>
        public FortuneConditionConverter(FortuneCalculator fortune)
        {
            _fortune = fortune ?? throw new ArgumentNullException(nameof(fortune));
        }

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override FortuneCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "area", "op", "value");
            return new FortuneCondition(
                _fortune,
                ContentReading.RequireEnum(json, "area", context, FortuneAreas.Names),
                RuleJson.RequireOperator(json, context),
                RuleJson.RequireFloat(json, "value", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(FortuneCondition condition, JObject json, RuleRegistry registry)
        {
            json["area"] = FortuneAreas.ToName(condition.Area);
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["value"] = RuleJson.Number(condition.Value);
        }
    }
}
