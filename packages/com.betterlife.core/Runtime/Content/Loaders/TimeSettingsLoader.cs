using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Time.Budget;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads the single content/time_settings.json and checks it against the activity catalog.
    /// </summary>
    public sealed class TimeSettingsLoader : IContentTypeLoader
    {
        private static readonly string[] Dependencies = { ContentTypes.Activities };

        /// <inheritdoc />
        public string TypeName => ContentTypes.TimeSettings;

        /// <inheritdoc />
        public IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        public bool AcceptsFiles => true;

        /// <inheritdoc />
        public void Load(IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
        {
            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (documents.Count != 1)
            {
                context.ReportError(documents.ElementAtOrDefault(1), string.Empty, $"Exactly one '{TypeName}' file is required, found {documents.Count}.");
                return;
            }

            ContentDocument document = documents[0];
            var activities = context.Require<DefinitionCatalog<ActivityDefinition>>();
            context.Guard(document, RuleReadContext.RootPath, () => context.Add(Read(document.Root, context.ReadContext(), activities)));
        }

        private static TimeSettings Read(JObject root, RuleReadContext context, DefinitionCatalog<ActivityDefinition> activities)
        {
            RuleJson.RequireOnly(root, context, "defaultActivity", "learning", "defaultBudget", ContentReading.SchemaProperty);
            string defaultActivity = RuleJson.RequireId(root, "defaultActivity", context);
            if (!activities.Contains(defaultActivity))
            {
                throw context.Property("defaultActivity").Error($"unknown activity '{defaultActivity}'.");
            }

            var (learningToken, learningContext) = RuleJson.RequireProperty(root, "learning", context);
            JObject learning = RuleJson.RequireObject(learningToken, learningContext);
            RuleJson.RequireOnly(learning, learningContext, "multiplierAtZeroMental", "multiplierAtFullMental");

            var (budget, budgetContext) = RuleJson.RequireArrayProperty(root, "defaultBudget", context);
            var hours = new Dictionary<string, int>(StringComparer.Ordinal);
            for (int i = 0; i < budget.Count; i++)
            {
                RuleReadContext entryContext = budgetContext.Index(i);
                JObject entry = RuleJson.RequireObject(budget[i], entryContext);
                RuleJson.RequireOnly(entry, entryContext, "activity", "hours");
                string activityId = RuleJson.RequireId(entry, "activity", entryContext);
                int value = RuleJson.RequireInt(entry, "hours", entryContext);
                if (!activities.TryGet(activityId, out ActivityDefinition activity))
                {
                    throw entryContext.Property("activity").Error($"unknown activity '{activityId}'.");
                }
                if (activityId == defaultActivity)
                {
                    throw entryContext.Property("activity").Error("the default activity receives the remaining hours and cannot be budgeted.");
                }
                if (hours.ContainsKey(activityId))
                {
                    throw entryContext.Property("activity").Error($"'{activityId}' is listed more than once.");
                }
                if (value < activity.MinHours || value > activity.MaxHours)
                {
                    throw entryContext.Property("hours").Error($"must be within {activity.MinHours}-{activity.MaxHours} for '{activityId}'.");
                }
                hours.Add(activityId, value);
            }
            if (hours.Values.Sum() > TimeBudget.HoursPerWeek)
            {
                throw budgetContext.Error($"default budget exceeds {TimeBudget.HoursPerWeek} hours.");
            }

            return new TimeSettings(
                defaultActivity,
                RuleJson.RequireFloat(learning, "multiplierAtZeroMental", learningContext),
                RuleJson.RequireFloat(learning, "multiplierAtFullMental", learningContext),
                hours);
        }
    }
}
