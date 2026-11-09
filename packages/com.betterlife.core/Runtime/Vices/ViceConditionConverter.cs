using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content.Loaders;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// Reads {"type":"vice","vice":"gambling","measure":"addiction","op":"&gt;=","value":30} ("measure" defaults to addiction).
    /// Registered by the vices loader with the known vice ids.
    /// </summary>
    public sealed class ViceConditionConverter : ConditionConverter<ViceCondition>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "vice";

        private static readonly IReadOnlyDictionary<string, ViceMeasure> Measures = new Dictionary<string, ViceMeasure>
        {
            ["addiction"] = ViceMeasure.Addiction,
            ["tolerance"] = ViceMeasure.Tolerance,
            ["uses"] = ViceMeasure.Uses,
            ["cleanWeeks"] = ViceMeasure.CleanWeeks,
        };

        private readonly ICollection<string> _knownVices;

        /// <summary>
        /// Creates the converter.
        /// </summary>
        /// <param name="knownVices">Vice ids (may be filled after registration).</param>
        public ViceConditionConverter(ICollection<string> knownVices)
        {
            _knownVices = knownVices ?? throw new ArgumentNullException(nameof(knownVices));
        }

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <summary>
        /// Returns the JSON name of a measure.
        /// </summary>
        /// <param name="measure">Measure.</param>
        /// <returns>The name.</returns>
        public static string NameOf(ViceMeasure measure) => Measures.First(p => p.Value == measure).Key;

        /// <inheritdoc />
        protected override ViceCondition ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "vice", "measure", "op", "value");
            ViceMeasure measure = json["measure"] == null ? ViceMeasure.Addiction : ContentReading.RequireEnum(json, "measure", context, Measures);
            return new ViceCondition(RequireVice(json, context, _knownVices), measure, RuleJson.RequireOperator(json, context),
                RuleJson.RequireFloat(json, "value", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(ViceCondition condition, JObject json, RuleRegistry registry)
        {
            json["vice"] = condition.ViceId;
            json["measure"] = NameOf(condition.Measure);
            json["op"] = Comparison.ToSymbol(condition.Operator);
            json["value"] = RuleJson.Number(condition.Value);
        }

        internal static string RequireVice(JObject json, RuleReadContext context, ICollection<string> known)
        {
            string id = RuleJson.RequireId(json, "vice", context);
            if (!known.Contains(id))
            {
                throw context.Property("vice").Error($"unknown vice '{id}'.");
            }
            return id;
        }
    }
}
