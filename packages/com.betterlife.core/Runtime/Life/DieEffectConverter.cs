using System;
using System.Collections.Generic;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Reads {"type":"character.die","cause":id}. Registered by the death causes loader with every cause id.
    /// </summary>
    public sealed class DieEffectConverter : EffectConverter<DieEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "character.die";

        private readonly ICollection<string> _causes;

        /// <summary>
        /// Creates the converter.
        /// </summary>
        /// <param name="causes">Every death cause id.</param>
        public DieEffectConverter(ICollection<string> causes)
        {
            _causes = causes ?? throw new ArgumentNullException(nameof(causes));
        }

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override DieEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "cause");
            string cause = RuleJson.RequireId(json, "cause", context);
            if (!_causes.Contains(cause))
            {
                throw context.Property("cause").Error($"unknown death cause '{cause}'.");
            }
            return new DieEffect(cause);
        }

        /// <inheritdoc />
        protected override void WriteTyped(DieEffect effect, JObject json, RuleRegistry registry)
        {
            json["cause"] = effect.CauseId;
        }
    }
}
