using System.Collections.Generic;
using BetterLife.Core.Content.Loaders;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Reads <c>{ "type": "career.end", "reason": "quit" }</c> (reason: quit or laid_off).
    /// </summary>
    public sealed class EndCareerEffectConverter : EffectConverter<EndCareerEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "career.end";

        private static readonly IReadOnlyDictionary<string, JobEndReason> Reasons = new Dictionary<string, JobEndReason>
        {
            ["quit"] = JobEndReason.Quit,
            ["laid_off"] = JobEndReason.LaidOff,
        };

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override EndCareerEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "reason");
            return new EndCareerEffect(ContentReading.RequireEnum(json, "reason", context, Reasons));
        }

        /// <inheritdoc />
        protected override void WriteTyped(EndCareerEffect effect, JObject json, RuleRegistry registry)
        {
            json["reason"] = effect.Reason == JobEndReason.Quit ? "quit" : "laid_off";
        }
    }
}
