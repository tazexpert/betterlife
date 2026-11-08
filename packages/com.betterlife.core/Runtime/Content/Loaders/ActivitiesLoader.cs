using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Time.Budget;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/activities.json. Stat effects must target stored stats; conditions use the rules engine.
    /// </summary>
    public sealed class ActivitiesLoader : DefinitionLoader<ActivityDefinition>
    {
        private static readonly string[] Dependencies = { ContentTypes.RuleTypes, ContentTypes.Careers };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.Activities;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override string ItemsProperty => "activities";

        /// <inheritdoc />
        protected override ActivityDefinition ReadItem(JObject item, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(item, context, "id", "nameKey", "descKey", "category", "minHours", "maxHours", "costPerHour", "condition", "stats", "skills");
            ICondition condition = item["condition"] == null ? null : content.Rules.ParseCondition(item["condition"], context.Property("condition"));
            var (stats, statsContext) = RuleJson.RequireArrayProperty(item, "stats", context);
            var (skills, skillsContext) = RuleJson.RequireArrayProperty(item, "skills", context);
            return new ActivityDefinition(
                RuleJson.RequireId(item, "id", context),
                RuleJson.RequireId(item, "nameKey", context),
                RuleJson.RequireId(item, "descKey", context),
                RuleJson.RequireId(item, "category", context),
                RuleJson.RequireInt(item, "minHours", context),
                RuleJson.RequireInt(item, "maxHours", context),
                RuleJson.RequireDecimal(item, "costPerHour", context),
                condition,
                stats.Select((token, i) => ReadStat(token, statsContext.Index(i))).ToList(),
                skills.Select((token, i) => ReadSkill(token, skillsContext.Index(i))).ToList());
        }

        private static ActivityStatEffect ReadStat(JToken token, RuleReadContext context)
        {
            JObject effect = RuleJson.RequireObject(token, context);
            RuleJson.RequireOnly(effect, context, "stat", "perHour", "maxHours");
            return new ActivityStatEffect(
                RuleJson.RequireStat(effect, "stat", context, true),
                RuleJson.RequireFloat(effect, "perHour", context),
                RuleJson.OptionalInt(effect, "maxHours", context));
        }

        private static ActivitySkillEffect ReadSkill(JToken token, RuleReadContext context)
        {
            JObject effect = RuleJson.RequireObject(token, context);
            RuleJson.RequireOnly(effect, context, "skill", "gainPerHour", "maxHours");
            return new ActivitySkillEffect(
                RuleJson.RequireSkill(effect, "skill", context),
                RuleJson.RequireFloat(effect, "gainPerHour", context),
                RuleJson.OptionalInt(effect, "maxHours", context));
        }
    }
}
