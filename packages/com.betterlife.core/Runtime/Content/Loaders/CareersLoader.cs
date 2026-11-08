using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Careers;
using BetterLife.Core.Luck;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/careers.json into a catalog of <see cref="CareerTrackDefinition"/> and registers the "career" condition
    /// (track ids are collected first so tracks, activities and events can refer to each other).
    /// </summary>
    public sealed class CareersLoader : DefinitionLoader<CareerTrackDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.RuleTypes };

        private readonly CareerTrackIndex _index = new CareerTrackIndex();

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Careers;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "tracks";

        /// <inheritdoc />
        protected override void BeforeItems(IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
        {
            _index.Clear();
            foreach (ContentDocument document in documents)
            {
                if (!(document.Root[ItemsProperty] is JArray items))
                {
                    continue;
                }
                foreach (JObject item in items.OfType<JObject>())
                {
                    if (item["id"]?.Type != JTokenType.String)
                    {
                        continue;
                    }
                    CareerCategory? category = item["category"]?.Type == JTokenType.String
                        && CareerNames.Categories.TryGetValue((string)item["category"], out CareerCategory value) ? value : (CareerCategory?)null;
                    IEnumerable<string> levels = (item["levels"] as JArray)?.OfType<JObject>()
                        .Where(l => l["id"]?.Type == JTokenType.String).Select(l => (string)l["id"]) ?? Enumerable.Empty<string>();
                    _index.Add((string)item["id"], category, levels);
                }
            }
            if (!context.Rules.ConditionTypes.Contains(CareerConditionConverter.TypeName))
            {
                context.Rules.Register(new CareerConditionConverter(_index));
            }
        }

        /// <inheritdoc />
        protected override CareerTrackDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "category", "premium", "requirement", "hiring", "weeklyHours", "performanceSkill",
                "skills", "stressPerHour", "incomeVolatility", "marketBeta", "layoffSensitivity", "pensionRate", "risk", "levels");
            ICondition requirement = item["requirement"] == null || item["requirement"].Type == JTokenType.Null
                ? null
                : content.Rules.ParseCondition(item["requirement"], context.Property("requirement"));
            string skill = ContentReading.RequireNullableId(item, "performanceSkill", context);
            if (skill != null)
            {
                RuleJson.RequireSkill(item, "performanceSkill", context);
            }
            var (levels, levelsContext) = RuleJson.RequireArrayProperty(item, "levels", context);
            return new CareerTrackDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                ContentReading.RequireEnum(item, "category", context, CareerNames.Categories),
                ContentReading.RequireBool(item, "premium", context),
                requirement,
                ChanceSpecReader.Read(item["hiring"], context.Property("hiring")),
                RuleJson.RequireInt(item, "weeklyHours", context),
                skill,
                ContentReading.ReadSkillEffects(item, "skills", context),
                RuleJson.RequireFloat(item, "stressPerHour", context),
                RuleJson.RequireFloat(item, "incomeVolatility", context),
                RuleJson.RequireFloat(item, "marketBeta", context),
                RuleJson.RequireFloat(item, "layoffSensitivity", context),
                RuleJson.RequireFloat(item, "pensionRate", context),
                ReadRisk(item, context, content),
                levels.Select((token, i) => ReadLevel(RuleJson.RequireObject(token, levelsContext.Index(i)), levelsContext.Index(i))).ToList());
        }

        private static CareerRisk ReadRisk(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            if (item["risk"] == null || item["risk"].Type == JTokenType.Null)
            {
                return null;
            }
            RuleReadContext riskContext = context.Property("risk");
            JObject risk = RuleJson.RequireObject(item["risk"], riskContext);
            RuleJson.RequireOnly(risk, riskContext, "monthlyChance", "endsJob", "reasonKey", "effects");
            return new CareerRisk(
                RuleJson.RequireFloat(risk, "monthlyChance", riskContext),
                ContentReading.RequireBool(risk, "endsJob", riskContext),
                RuleJson.RequireId(risk, "reasonKey", riskContext),
                content.Rules.ParseEffects(risk["effects"], riskContext.Property("effects")));
        }

        private static CareerLevelDefinition ReadLevel(JObject level, RuleReadContext context)
        {
            RuleJson.RequireOnly(level, context, "id", "titleKey", "monthlySalary", "promotionAfterMonths", "promotionPerformance");
            return new CareerLevelDefinition(
                RuleJson.RequireId(level, "id", context),
                RuleJson.RequireId(level, "titleKey", context),
                RuleJson.RequireDecimal(level, "monthlySalary", context),
                ContentReading.RequireNullableInt(level, "promotionAfterMonths", context),
                RuleJson.RequireFloat(level, "promotionPerformance", context));
        }
    }
}
