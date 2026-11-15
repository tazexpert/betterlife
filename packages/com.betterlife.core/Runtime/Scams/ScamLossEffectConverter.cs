using System;
using System.Collections.Generic;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// Reads {"type":"scam.loss","scam":"ponzi_scheme","cashShare":0.5,"min":5000,"max":2000000}. Registered by the scams loader.
    /// </summary>
    public sealed class ScamLossEffectConverter : EffectConverter<ScamLossEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "scam.loss";

        private readonly ICollection<string> _knownScams;

        /// <summary>
        /// Creates the converter.
        /// </summary>
        /// <param name="knownScams">Scam ids (may be filled after registration).</param>
        public ScamLossEffectConverter(ICollection<string> knownScams)
        {
            _knownScams = knownScams ?? throw new ArgumentNullException(nameof(knownScams));
        }

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override ScamLossEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "scam", "cashShare", "min", "max");
            string id = RuleJson.RequireId(json, "scam", context);
            if (!_knownScams.Contains(id))
            {
                throw context.Property("scam").Error($"unknown scam '{id}'.");
            }
            return new ScamLossEffect(id, RuleJson.RequireDecimal(json, "cashShare", context), RuleJson.RequireDecimal(json, "min", context),
                RuleJson.RequireDecimal(json, "max", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(ScamLossEffect effect, JObject json, RuleRegistry registry)
        {
            json["scam"] = effect.ScamId;
            json["cashShare"] = RuleJson.Number(effect.CashShare);
            json["min"] = RuleJson.Number(effect.Min);
            json["max"] = RuleJson.Number(effect.Max);
        }
    }
}
