using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Vices;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/vices.json into a catalog of <see cref="ViceDefinition"/> and registers the "vice" condition and
    /// "vice.addiction" effect (vice ids are collected first so events can refer to them).
    /// </summary>
    public sealed class VicesLoader : DefinitionLoader<ViceDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.RuleTypes, ContentTypes.Activities };

        private readonly HashSet<string> _known = new HashSet<string>(StringComparer.Ordinal);

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Vices;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "vices";

        /// <inheritdoc />
        protected override void BeforeItems(IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
        {
            _known.Clear();
            foreach (JObject item in documents.Select(d => d.Root[ItemsProperty]).OfType<JArray>().SelectMany(a => a.OfType<JObject>()))
            {
                if (item["id"]?.Type == JTokenType.String)
                {
                    _known.Add((string)item["id"]);
                }
            }
            if (!context.Rules.ConditionTypes.Contains(ViceConditionConverter.TypeName))
            {
                context.Rules.Register(new ViceConditionConverter(_known));
            }
            if (!context.Rules.EffectTypes.Contains(ViceAddictionEffectConverter.TypeName))
            {
                context.Rules.Register(new ViceAddictionEffectConverter(_known));
            }
        }

        /// <inheritdoc />
        protected override ViceDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "minAge", "blocksUndo", "activity", "temptation", "costPerUse", "relief", "addiction",
                "withdrawalFrom", "withdrawal", "useEffects", "risks", "discovery", "gambling");
            var (temptation, tContext) = SettingsReading.Section(item, "temptation", context, "baseWeekly", "stages");
            var stages = content.Require<CharacterCatalogs>().LifeStages;
            var (stageArray, stagesContext) = RuleJson.RequireArrayProperty(temptation, "stages", tContext);
            var multipliers = new Dictionary<string, float>(StringComparer.Ordinal);
            for (int i = 0; i < stageArray.Count; i++)
            {
                RuleReadContext c = stagesContext.Index(i);
                JObject stage = RuleJson.RequireObject(stageArray[i], c);
                RuleJson.RequireOnly(stage, c, "stage", "multiplier");
                string id = SettingsReading.RequireKnown(stage, "stage", c, s => stages.All.Any(x => x.Id == s), "life stage");
                multipliers[id] = RuleJson.RequireFloat(stage, "multiplier", c);
            }
            var (relief, rContext) = SettingsReading.Section(item, "relief", context, "stress", "happiness");
            var (addiction, aContext) = SettingsReading.Section(item, "addiction", context, "gainPerUse", "decayPerCleanWeek", "tolerancePerUse", "toleranceDecay");
            var (withdrawal, wContext) = SettingsReading.Section(item, "withdrawal", context, "stress", "happiness");
            var (risks, risksContext) = RuleJson.RequireArrayProperty(item, "risks", context);
            return new ViceDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireInt(item, "minAge", context),
                ContentReading.RequireBool(item, "blocksUndo", context),
                ReadActivity(item, context, content),
                RuleJson.RequireFloat(temptation, "baseWeekly", tContext),
                multipliers,
                RuleJson.RequireDecimal(item, "costPerUse", context),
                RuleJson.RequireFloat(relief, "stress", rContext),
                RuleJson.RequireFloat(relief, "happiness", rContext),
                RuleJson.RequireFloat(addiction, "gainPerUse", aContext),
                RuleJson.RequireFloat(addiction, "decayPerCleanWeek", aContext),
                RuleJson.RequireFloat(addiction, "tolerancePerUse", aContext),
                RuleJson.RequireFloat(addiction, "toleranceDecay", aContext),
                RuleJson.RequireFloat(item, "withdrawalFrom", context),
                RuleJson.RequireFloat(withdrawal, "stress", wContext),
                RuleJson.RequireFloat(withdrawal, "happiness", wContext),
                content.Rules.ParseEffects(item["useEffects"], context.Property("useEffects")),
                risks.Select((token, i) => ReadRisk(RuleJson.RequireObject(token, risksContext.Index(i)), risksContext.Index(i), content)).ToList(),
                ReadDiscovery(item, context, content),
                ReadGambling(item, context));
        }

        private static ViceActivityLink ReadActivity(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            if (IsNull(item["activity"]))
            {
                return null;
            }
            var (link, c) = SettingsReading.Section(item, "activity", context, "id", "freeHours", "hoursPerUse");
            var activities = content.Require<DefinitionCatalog<ActivityDefinition>>();
            return new ViceActivityLink(SettingsReading.RequireKnown(link, "id", c, activities.Contains, "activity"),
                RuleJson.RequireInt(link, "freeHours", c), RuleJson.RequireInt(link, "hoursPerUse", c));
        }

        private static ViceRisk ReadRisk(JObject risk, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(risk, context, "chance", "reasonKey", "severity", "effects");
            return new ViceRisk(RuleJson.RequireFloat(risk, "chance", context), RuleJson.RequireId(risk, "reasonKey", context),
                ContentReading.RequireEnum(risk, "severity", context, SeverityNames.All), content.Rules.ParseEffects(risk["effects"], context.Property("effects")));
        }

        private static ViceDiscovery ReadDiscovery(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            if (IsNull(item["discovery"]))
            {
                return null;
            }
            var (discovery, c) = SettingsReading.Section(item, "discovery", context, "fromAddiction", "weeklyChance", "effects");
            return new ViceDiscovery(RuleJson.RequireFloat(discovery, "fromAddiction", c), RuleJson.RequireFloat(discovery, "weeklyChance", c),
                content.Rules.ParseEffects(discovery["effects"], c.Property("effects")));
        }

        private static GamblingRules ReadGambling(JObject item, RuleReadContext context)
        {
            if (IsNull(item["gambling"]))
            {
                return null;
            }
            var (gambling, c) = SettingsReading.Section(item, "gambling", context, "stake", "winChance", "payout", "beginnerBets", "beginnerWinChance");
            return new GamblingRules(RuleJson.RequireDecimalRange(gambling, "stake", c), RuleJson.RequireFloat(gambling, "winChance", c),
                RuleJson.RequireDecimal(gambling, "payout", c), RuleJson.RequireInt(gambling, "beginnerBets", c), RuleJson.RequireFloat(gambling, "beginnerWinChance", c));
        }

        private static bool IsNull(JToken token) => token == null || token.Type == JTokenType.Null;
    }
}
