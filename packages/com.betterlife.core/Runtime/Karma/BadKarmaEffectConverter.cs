using System;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Reads {"type":"karma.bad","amount":8 or {min,max}}. Registered by the luck settings loader.
    /// </summary>
    public sealed class BadKarmaEffectConverter : EffectConverter<BadKarmaEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "karma.bad";

        private readonly KarmaService _karma;

        /// <summary>
        /// Creates the converter.
        /// </summary>
        /// <param name="karma">Karma service injected into parsed effects.</param>
        public BadKarmaEffectConverter(KarmaService karma)
        {
            _karma = karma ?? throw new ArgumentNullException(nameof(karma));
        }

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override BadKarmaEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "amount");
            return new BadKarmaEffect(_karma, RuleJson.RequireFloatRange(json, "amount", context));
        }

        /// <inheritdoc />
        protected override void WriteTyped(BadKarmaEffect effect, JObject json, RuleRegistry registry)
        {
            json["amount"] = RuleJson.Range(effect.Amount);
        }
    }
}
