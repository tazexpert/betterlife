using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Careers;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Luck;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Time.Budget;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/career_settings.json into <see cref="CareerSettings"/> and checks the activity and economy phases it names.
    /// </summary>
    public sealed class CareerSettingsLoader : SingleDocumentLoader
    {
        private static readonly string[] Dependencies =
        {
            ContentTypes.Stats, ContentTypes.CharacterCatalogs, ContentTypes.Activities, ContentTypes.Economy, ContentTypes.Astrology,
        };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.CareerSettings;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "workActivity", "minWorkAge", "retirementAge", "performance", "boss", "hiring", "promotion", "layoffs",
                ContentReading.SchemaProperty);
            SettingsReading.RequireStats(content.Require<StatCatalog>(), new[] { StatIds.Knowledge, StatIds.Mental, StatIds.Fatigue, StatIds.Stress }, context, "careers");
            var activities = content.Require<DefinitionCatalog<ActivityDefinition>>();
            string work = SettingsReading.RequireKnown(root, "workActivity", context, activities.Contains, "activity");
            var (performance, performanceContext) = SettingsReading.Section(root, "performance", context,
                "weeklySmoothing", "attendanceWeight", "skillWeight", "mentalWeight", "restWeight", "start");
            var (boss, bossContext) = SettingsReading.Section(root, "boss", context, "weeklySmoothing", "start", "fireBelow", "fireAfterMonths", "stressFromDissatisfaction");
            var (hiring, hiringContext) = SettingsReading.Section(root, "hiring", context, "prestigePerPoint", "retryWeeks");
            var (promotion, promotionContext) = SettingsReading.Section(root, "promotion", context, "chance", "perPointAbove", "minBoss", "reviewEveryMonths");

            EconomySettings economy = content.Require<EconomySettings>();
            var (layoffs, layoffsContext) = RuleJson.RequireArrayProperty(root, "layoffs", context);
            var layoffChances = new Dictionary<string, float>();
            for (int i = 0; i < layoffs.Count; i++)
            {
                RuleReadContext c = layoffsContext.Index(i);
                JObject entry = RuleJson.RequireObject(layoffs[i], c);
                RuleJson.RequireOnly(entry, c, "phase", "monthlyChance");
                string phase = SettingsReading.RequireKnown(entry, "phase", c, economy.Phases.Contains, "economy phase");
                if (layoffChances.ContainsKey(phase))
                {
                    throw c.Property("phase").Error($"duplicate phase '{phase}'.");
                }
                layoffChances.Add(phase, RuleJson.RequireFloat(entry, "monthlyChance", c));
            }

            content.Add(new CareerSettings(
                work,
                RuleJson.RequireInt(root, "minWorkAge", context),
                RuleJson.RequireInt(root, "retirementAge", context),
                new WorkPerformanceWeights(
                    RuleJson.RequireFloat(performance, "weeklySmoothing", performanceContext),
                    RuleJson.RequireFloat(performance, "attendanceWeight", performanceContext),
                    RuleJson.RequireFloat(performance, "skillWeight", performanceContext),
                    RuleJson.RequireFloat(performance, "mentalWeight", performanceContext),
                    RuleJson.RequireFloat(performance, "restWeight", performanceContext),
                    RuleJson.RequireFloat(performance, "start", performanceContext)),
                new BossRules(
                    RuleJson.RequireFloat(boss, "weeklySmoothing", bossContext),
                    RuleJson.RequireFloat(boss, "start", bossContext),
                    RuleJson.RequireFloat(boss, "fireBelow", bossContext),
                    RuleJson.RequireInt(boss, "fireAfterMonths", bossContext),
                    RuleJson.RequireFloat(boss, "stressFromDissatisfaction", bossContext)),
                RuleJson.RequireFloat(hiring, "prestigePerPoint", hiringContext),
                RuleJson.RequireInt(hiring, "retryWeeks", hiringContext),
                new PromotionRules(
                    ChanceSpecReader.Read(promotion["chance"], promotionContext.Property("chance")),
                    RuleJson.RequireFloat(promotion, "perPointAbove", promotionContext),
                    RuleJson.RequireFloat(promotion, "minBoss", promotionContext),
                    RuleJson.RequireInt(promotion, "reviewEveryMonths", promotionContext)),
                layoffChances));
        }
    }
}
