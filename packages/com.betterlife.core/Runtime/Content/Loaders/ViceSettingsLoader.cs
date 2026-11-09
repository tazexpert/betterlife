using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.Traits;
using BetterLife.Core.Luck;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Vices;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/vice_settings.json into <see cref="ViceSettings"/> and checks the traits and activities it names.
    /// </summary>
    public sealed class ViceSettingsLoader : SingleDocumentLoader
    {
        private static readonly string[] Dependencies =
        {
            ContentTypes.Stats, ContentTypes.Traits, ContentTypes.Activities, ContentTypes.CharacterCatalogs, ContentTypes.Astrology,
        };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.ViceSettings;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "visibleFromAddiction", "firstTryMultiplier", "chaseFromAddiction", "maxWeeklyTemptation", "stress",
                "addictionWeight", "resist", "support", "badCompany", "laziness", "traits", "helplineKey", ContentReading.SchemaProperty);
            SettingsReading.RequireStats(content.Require<StatCatalog>(), new[] { StatIds.Stress, StatIds.Happiness, StatIds.Willpower }, context, "vices");
            var traits = content.Require<DefinitionCatalog<TraitDefinition>>();
            var activities = content.Require<DefinitionCatalog<ActivityDefinition>>();
            var (stress, sContext) = SettingsReading.Section(root, "stress", context, "weight", "minMultiplier");
            var (resist, rContext) = SettingsReading.Section(root, "resist", context, "chance", "perAddiction", "willpowerGain");
            var (support, suContext) = SettingsReading.Section(root, "support", context, "minValue", "perTie", "max");
            var (bad, bContext) = SettingsReading.Section(root, "badCompany", context, "traits", "minValue", "perFriend", "max");
            var (lazy, lContext) = SettingsReading.Section(root, "laziness", context, "activities", "baselineHours", "perHour", "max");
            var (traitArray, traitsContext) = RuleJson.RequireArrayProperty(root, "traits", context);
            var traitMultipliers = new Dictionary<string, float>(StringComparer.Ordinal);
            for (int i = 0; i < traitArray.Count; i++)
            {
                RuleReadContext c = traitsContext.Index(i);
                JObject entry = RuleJson.RequireObject(traitArray[i], c);
                RuleJson.RequireOnly(entry, c, "trait", "multiplier");
                traitMultipliers[SettingsReading.RequireKnown(entry, "trait", c, traits.Contains, "trait")] = RuleJson.RequireFloat(entry, "multiplier", c);
            }

            content.Add(new ViceSettings(
                RuleJson.RequireFloat(root, "visibleFromAddiction", context),
                RuleJson.RequireFloat(root, "firstTryMultiplier", context),
                RuleJson.RequireFloat(root, "chaseFromAddiction", context),
                RuleJson.RequireFloat(root, "maxWeeklyTemptation", context),
                RuleJson.RequireFloat(stress, "weight", sContext),
                RuleJson.RequireFloat(stress, "minMultiplier", sContext),
                RuleJson.RequireFloat(root, "addictionWeight", context),
                new ResistRules(ChanceSpecReader.Read(resist["chance"], rContext.Property("chance")), RuleJson.RequireFloat(resist, "perAddiction", rContext),
                    RuleJson.RequireFloat(resist, "willpowerGain", rContext)),
                RuleJson.RequireFloat(support, "minValue", suContext),
                RuleJson.RequireFloat(support, "perTie", suContext),
                RuleJson.RequireFloat(support, "max", suContext),
                new BadCompanyRules(ReadIds(bad, "traits", bContext, traits.Contains, "trait"), RuleJson.RequireFloat(bad, "minValue", bContext),
                    RuleJson.RequireFloat(bad, "perFriend", bContext), RuleJson.RequireFloat(bad, "max", bContext)),
                new LazinessRules(ReadIds(lazy, "activities", lContext, activities.Contains, "activity"), RuleJson.RequireInt(lazy, "baselineHours", lContext),
                    RuleJson.RequireFloat(lazy, "perHour", lContext), RuleJson.RequireFloat(lazy, "max", lContext)),
                traitMultipliers,
                RuleJson.RequireId(root, "helplineKey", context)));
        }

        private static IReadOnlyCollection<string> ReadIds(JObject obj, string name, RuleReadContext context, Func<string, bool> exists, string what)
        {
            var (array, arrayContext) = RuleJson.RequireArrayProperty(obj, name, context);
            return array.Select((token, i) =>
            {
                string id = token.Type == JTokenType.String ? (string)token : null;
                if (id == null || !exists(id))
                {
                    throw arrayContext.Index(i).Error($"unknown {what} '{token}'.");
                }
                return id;
            }).ToList();
        }
    }
}
