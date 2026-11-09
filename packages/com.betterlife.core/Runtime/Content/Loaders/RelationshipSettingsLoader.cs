using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Relationships;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Time.Budget;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/relationship_settings.json into <see cref="RelationshipSettings"/> and checks the activity and parent personalities it names.
    /// </summary>
    public sealed class RelationshipSettingsLoader : SingleDocumentLoader
    {
        private static readonly string[] Dependencies = { ContentTypes.Stats, ContentTypes.Activities, ContentTypes.ParentPersonalities };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.RelationshipSettings;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "socialActivity", "maxRelationships", "types", "attention", "family", "meeting", "compatibility",
                "romance", "children", "stats", "network", ContentReading.SchemaProperty);
            SettingsReading.RequireStats(content.Require<StatCatalog>(), new[] { StatIds.Stress, StatIds.Happiness }, context, "relationships");
            var activities = content.Require<DefinitionCatalog<ActivityDefinition>>();
            string social = SettingsReading.RequireKnown(root, "socialActivity", context, activities.Contains, "activity");

            var (types, typesContext) = RuleJson.RequireArrayProperty(root, "types", context);
            List<RelationshipTypeRules> typeRules = types.Select((token, i) =>
            {
                RuleReadContext c = typesContext.Index(i);
                JObject item = RuleJson.RequireObject(token, c);
                RuleJson.RequireOnly(item, c, "type", "monthlyDecay", "floor", "griefStress", "griefHappiness");
                return new RelationshipTypeRules(ContentReading.RequireEnum(item, "type", c, RelationshipNames.Types),
                    RuleJson.RequireFloat(item, "monthlyDecay", c), RuleJson.RequireFloat(item, "floor", c),
                    RuleJson.RequireFloat(item, "griefStress", c), RuleJson.RequireFloat(item, "griefHappiness", c));
            }).ToList();
            if (typeRules.Select(t => t.Type).Distinct().Count() != typeRules.Count)
            {
                throw typesContext.Error("each relationship type may appear only once.");
            }

            var (attention, aContext) = SettingsReading.Section(root, "attention", context, "gainPerHour", "closestCount", "familyMonthlyGain");
            var (meeting, mContext) = SettingsReading.Section(root, "meeting", context, "monthlyBase", "perSocialHour", "school", "work", "startValue", "ageGapYears");
            var (compat, cContext) = SettingsReading.Section(root, "compatibility", context, "base", "sharedTrait", "conflictingTrait", "perYearAgeGap", "looksWeight", "charismaWeight");
            var (romance, rContext) = SettingsReading.Section(root, "romance", context, "breakupBelow", "breakupAfterMonths", "divorceBelow", "divorceAfterMonths");
            var (children, chContext) = SettingsReading.Section(root, "children", context, "fertileAge", "pregnancyWeeks", "monthlyCost", "costUntilAge");
            var (fertile, fContext) = SettingsReading.Section(children, "fertileAge", chContext, "min", "max");
            var (stats, sContext) = SettingsReading.Section(root, "stats", context, "closeValue", "topCount", "emptySlotValue", "monthlySmoothing", "lonelinessBase", "lonelinessPerCloseTie");
            var (network, nContext) = SettingsReading.Section(root, "network", context, "minValue", "pointsPerContact", "maxPoints");

            content.Add(new RelationshipSettings(
                social,
                RuleJson.RequireInt(root, "maxRelationships", context),
                typeRules,
                new AttentionRules(RuleJson.RequireFloat(attention, "gainPerHour", aContext), RuleJson.RequireInt(attention, "closestCount", aContext),
                    RuleJson.RequireFloat(attention, "familyMonthlyGain", aContext)),
                ReadFamily(root, context, content),
                new MeetingRules(RuleJson.RequireFloat(meeting, "monthlyBase", mContext), RuleJson.RequireFloat(meeting, "perSocialHour", mContext),
                    RuleJson.RequireFloat(meeting, "school", mContext), RuleJson.RequireFloat(meeting, "work", mContext),
                    RuleJson.RequireFloatRange(meeting, "startValue", mContext), RuleJson.RequireInt(meeting, "ageGapYears", mContext)),
                new CompatibilityRules(RuleJson.RequireFloat(compat, "base", cContext), RuleJson.RequireFloat(compat, "sharedTrait", cContext),
                    RuleJson.RequireFloat(compat, "conflictingTrait", cContext), RuleJson.RequireFloat(compat, "perYearAgeGap", cContext),
                    RuleJson.RequireFloat(compat, "looksWeight", cContext), RuleJson.RequireFloat(compat, "charismaWeight", cContext)),
                new RomanceRules(RuleJson.RequireFloat(romance, "breakupBelow", rContext), RuleJson.RequireInt(romance, "breakupAfterMonths", rContext),
                    RuleJson.RequireFloat(romance, "divorceBelow", rContext), RuleJson.RequireInt(romance, "divorceAfterMonths", rContext)),
                new ChildRules(RuleJson.RequireInt(fertile, "min", fContext), RuleJson.RequireInt(fertile, "max", fContext),
                    RuleJson.RequireInt(children, "pregnancyWeeks", chContext), RuleJson.RequireDecimal(children, "monthlyCost", chContext),
                    RuleJson.RequireInt(children, "costUntilAge", chContext)),
                new RelationshipStatRules(RuleJson.RequireFloat(stats, "closeValue", sContext), RuleJson.RequireInt(stats, "topCount", sContext),
                    RuleJson.RequireFloat(stats, "emptySlotValue", sContext), RuleJson.RequireFloat(stats, "monthlySmoothing", sContext),
                    RuleJson.RequireFloat(stats, "lonelinessBase", sContext), RuleJson.RequireFloat(stats, "lonelinessPerCloseTie", sContext)),
                new NetworkRules(RuleJson.RequireFloat(network, "minValue", nContext), RuleJson.RequireFloat(network, "pointsPerContact", nContext),
                    RuleJson.RequireFloat(network, "maxPoints", nContext))));
        }

        private static FamilyRules ReadFamily(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            var (family, familyContext) = SettingsReading.Section(root, "family", context,
                "parentStart", "defaultParentStart", "siblingStart", "childStart", "siblings", "siblingAgeGapYears");
            var personalities = content.Require<DefinitionCatalog<ParentPersonalityDefinition>>();
            var (parents, parentsContext) = RuleJson.RequireArrayProperty(family, "parentStart", familyContext);
            var parentStart = new Dictionary<string, float>();
            for (int i = 0; i < parents.Count; i++)
            {
                RuleReadContext c = parentsContext.Index(i);
                JObject item = RuleJson.RequireObject(parents[i], c);
                RuleJson.RequireOnly(item, c, "personality", "value");
                string id = SettingsReading.RequireKnown(item, "personality", c, personalities.Contains, "parent personality");
                parentStart[id] = RuleJson.RequireFloat(item, "value", c);
            }
            var (siblings, siblingsContext) = RuleJson.RequireArrayProperty(family, "siblings", familyContext);
            List<(int, float)> weights = siblings.Select((token, i) =>
            {
                RuleReadContext c = siblingsContext.Index(i);
                JObject item = RuleJson.RequireObject(token, c);
                RuleJson.RequireOnly(item, c, "count", "weight");
                return (RuleJson.RequireInt(item, "count", c), RuleJson.RequireFloat(item, "weight", c));
            }).ToList();
            return new FamilyRules(parentStart, RuleJson.RequireFloat(family, "defaultParentStart", familyContext),
                RuleJson.RequireFloat(family, "siblingStart", familyContext), RuleJson.RequireFloat(family, "childStart", familyContext),
                weights, RuleJson.RequireInt(family, "siblingAgeGapYears", familyContext));
        }
    }
}
