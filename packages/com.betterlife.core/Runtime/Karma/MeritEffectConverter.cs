using System;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Reads {"type":"karma.merit","amount":10 or {min,max}}. Registered by the luck settings loader.
    /// </summary>
    public sealed class MeritEffectConverter : EffectConverter<MeritEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "karma.merit";

        private readonly KarmaService _karma;

        /// <summary>
        /// Creates the converter.
        /// </summary>
        /// <param name="karma">Karma service injected into parsed effects.</param>
        public MeritEffectConverter(KarmaService karma)
        {
            _karma = karma ?? throw new ArgumentNullException(nameof(karma));
        }

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override MeritEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "amount");
            return new MeritEffect(_karma, RuleJson.RequireFloatRange(json, "amount", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(MeritEffect effect, JObject json, RuleRegistry registry)
        {
            json["amount"] = RuleJson.Range(effect.Amount);
        }
    }
}
