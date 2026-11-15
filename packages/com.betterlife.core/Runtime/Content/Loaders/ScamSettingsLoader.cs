using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Scams;
using BetterLife.Core.Time.Budget;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/scam_settings.json into <see cref="ScamSettings"/> and checks the activities and stats it relies on.
    /// </summary>
    public sealed class ScamSettingsLoader : SingleDocumentLoader
    {
        private static readonly string[] Dependencies = { ContentTypes.Stats, ContentTypes.Activities };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.ScamSettings;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "onlineActivities", "onlineBaselineHours", "perOnlineHour", "maxOnline", "wealthReference",
                "stressPenaltyPerPoint", "support", "lessonGain", "detectedGain", "maxIncidents", "maxMonthlyChance", "helplineKey", "majorLoss", ContentReading.SchemaProperty);
            SettingsReading.RequireStats(content.Require<StatCatalog>(), new[] { StatIds.Stress, StatIds.StreetSmarts }, context, "scams");
            var activities = content.Require<DefinitionCatalog<ActivityDefinition>>();
            var (online, onlineContext) = RuleJson.RequireArrayProperty(root, "onlineActivities", context);
            List<string> onlineIds = online.Select((token, i) =>
            {
                string id = token.Type == JTokenType.String ? (string)token : null;
                if (id == null || !activities.Contains(id))
                {
                    throw onlineContext.Index(i).Error($"unknown activity '{token}'.");
                }
                return id;
            }).ToList();
            var (support, supportContext) = SettingsReading.Section(root, "support", context, "minValue", "perTie", "max");
            content.Add(new ScamSettings(
                onlineIds,
                RuleJson.RequireInt(root, "onlineBaselineHours", context),
                RuleJson.RequireFloat(root, "perOnlineHour", context),
                RuleJson.RequireFloat(root, "maxOnline", context),
                RuleJson.RequireDecimal(root, "wealthReference", context),
                RuleJson.RequireFloat(root, "stressPenaltyPerPoint", context),
                RuleJson.RequireFloat(support, "minValue", supportContext),
                RuleJson.RequireFloat(support, "perTie", supportContext),
                RuleJson.RequireFloat(support, "max", supportContext),
                RuleJson.RequireFloat(root, "lessonGain", context),
                RuleJson.RequireFloat(root, "detectedGain", context),
                RuleJson.RequireInt(root, "maxIncidents", context),
                RuleJson.RequireFloat(root, "maxMonthlyChance", context),
                RuleJson.RequireId(root, "helplineKey", context),
                RuleJson.RequireDecimal(root, "majorLoss", context)));
        }
    }
}
