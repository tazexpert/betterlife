using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content.Loaders;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// Reads {"type":"scam","measure":"victims"|"lost"|"detected","op":"&gt;=","value":1}.
    /// </summary>
    public sealed class ScamConditionConverter : ConditionConverter<ScamCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "scam";

        private static readonly IReadOnlyDictionary<string, ScamMeasure> Measures = new Dictionary<string, ScamMeasure>
        {
            ["victims"] = ScamMeasure.Victims,
            ["lost"] = ScamMeasure.Lost,
            ["detected"] = ScamMeasure.Detected,
        };

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        /// Returns the JSON name of a measure.
        /// </summary>
        /// <param name="measure">Measure.</param>
        /// <returns>The name.</returns>
        public static string NameOf(ScamMeasure measure) => Measures.First(p => p.Value == measure).Key;

        /// <inheritdoc />
        protected override ScamCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "measure", "op", "value");
            return new ScamCondition(ContentReading.RequireEnum(json, "measure", context, Measures), RuleJson.RequireOperator(json, context),
                RuleJson.RequireDecimal(json, "value", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(ScamCondition condition, JObject json, RuleRegistry registry)
        {
            json["measure"] = NameOf(condition.Measure);
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["value"] = RuleJson.Number(condition.Value);
        }
    }
}
