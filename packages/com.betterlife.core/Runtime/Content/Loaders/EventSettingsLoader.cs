using System.Collections.Generic;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads the single content/event_settings.json.
    /// </summary>
    public sealed class EventSettingsLoader : SingleDocumentLoader
    {
        private static readonly string[] NoDependencies = new string[0];

        /// <inheritdoc />
        public override string TypeName => ContentTypes.EventSettings;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => NoDependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "weeklyEventChance", "responseWindowWeeks", "antiFrustration", ContentReading.SchemaProperty);
            var (token, afContext) = RuleJson.RequireProperty(root, "antiFrustration", context);
            JObject af = RuleJson.RequireObject(token, afContext);
            RuleJson.RequireOnly(af, afContext, "streakThreshold", "windowWeeks", "negativeWeightMultiplier");
            content.Add(new EventSettings(
                RuleJson.RequireFloat(root, "weeklyEventChance", context),
                RuleJson.RequireInt(root, "responseWindowWeeks", context),
                new AntiFrustrationSettings(
                    RuleJson.RequireInt(af, "streakThreshold", afContext),
                    RuleJson.RequireInt(af, "windowWeeks", afContext),
                    RuleJson.RequireFloat(af, "negativeWeightMultiplier", afContext))));
        }
    }
}
