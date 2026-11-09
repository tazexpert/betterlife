using System;
using System.Collections.Generic;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// Reads {"type":"vice.addiction","vice":"drinking","amount":5 or {min,max}}. Registered by the vices loader.
    /// </summary>
    public sealed class ViceAddictionEffectConverter : EffectConverter<ViceAddictionEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "vice.addiction";

        private readonly ICollection<string> _knownVices;

        /// <summary>
        /// Creates the converter.
        /// </summary>
        /// <param name="knownVices">Vice ids (may be filled after registration).</param>
        public ViceAddictionEffectConverter(ICollection<string> knownVices)
        {
            _knownVices = knownVices ?? throw new ArgumentNullException(nameof(knownVices));
        }

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override ViceAddictionEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "vice", "amount");
            return new ViceAddictionEffect(ViceConditionConverter.RequireVice(json, context, _knownVices), RuleJson.RequireFloatRange(json, "amount", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(ViceAddictionEffect effect, JObject json, RuleRegistry registry)
        {
            json["vice"] = effect.ViceId;
            json["amount"] = RuleJson.Range(effect.Amount);
        }
    }
}
