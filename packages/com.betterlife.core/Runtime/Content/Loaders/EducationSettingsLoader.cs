using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Education;
using BetterLife.Core.Luck;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Time.Budget;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/education_settings.json into <see cref="EducationSettings"/> and checks the activities and loan product it names.
    /// </summary>
    public sealed class EducationSettingsLoader : SingleDocumentLoader
    {
        private static readonly string[] Dependencies =
        {
            ContentTypes.Stats, ContentTypes.CharacterCatalogs, ContentTypes.Activities, ContentTypes.DebtProducts, ContentTypes.Astrology,
        };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.EducationSettings;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "schoolActivity", "studyActivity", "startPerformance", "performance", "yearEnd", "admission",
                "scholarship", "studentLoanProduct", ContentReading.SchemaProperty);
            SettingsReading.RequireStats(content.Require<StatCatalog>(), new[] { StatIds.Iq, StatIds.Knowledge, StatIds.Mental }, context, "education");
            var activities = content.Require<DefinitionCatalog<ActivityDefinition>>();
            string school = SettingsReading.RequireKnown(root, "schoolActivity", context, activities.Contains, "activity");
            string study = SettingsReading.RequireKnown(root, "studyActivity", context, activities.Contains, "activity");
            var products = content.Require<DefinitionCatalog<DebtProductDefinition>>();
            string loan = SettingsReading.RequireKnown(root, "studentLoanProduct", context, products.Contains, "loan product");
            if (products.Get(loan).IsRevolving)
            {
                throw context.Property("studentLoanProduct").Error($"loan product '{loan}' must be an installment loan.");
            }

            var (performance, performanceContext) = SettingsReading.Section(root, "performance", context,
                "weeklySmoothing", "attendanceWeight", "iqWeight", "knowledgeWeight", "mentalWeight", "studyBonusPerHour", "maxStudyBonus");
            var (yearEnd, yearEndContext) = SettingsReading.Section(root, "yearEnd", context, "passPerformance", "maxRepeats");
            var (admission, admissionContext) = SettingsReading.Section(root, "admission", context,
                "chance", "perPointAboveDifficulty", "performanceWeight", "knowledgeWeight", "retryWeeks");
            var (scholarship, scholarshipContext) = SettingsReading.Section(root, "scholarship", context, "minScore", "chance", "keepPerformance");

            content.Add(new EducationSettings(
                school,
                study,
                RuleJson.RequireFloat(root, "startPerformance", context),
                new EducationPerformanceWeights(
                    RuleJson.RequireFloat(performance, "weeklySmoothing", performanceContext),
                    RuleJson.RequireFloat(performance, "attendanceWeight", performanceContext),
                    RuleJson.RequireFloat(performance, "iqWeight", performanceContext),
                    RuleJson.RequireFloat(performance, "knowledgeWeight", performanceContext),
                    RuleJson.RequireFloat(performance, "mentalWeight", performanceContext),
                    RuleJson.RequireFloat(performance, "studyBonusPerHour", performanceContext),
                    RuleJson.RequireFloat(performance, "maxStudyBonus", performanceContext)),
                RuleJson.RequireFloat(yearEnd, "passPerformance", yearEndContext),
                RuleJson.RequireInt(yearEnd, "maxRepeats", yearEndContext),
                new AdmissionRules(
                    ChanceSpecReader.Read(admission["chance"], admissionContext.Property("chance")),
                    RuleJson.RequireFloat(admission, "perPointAboveDifficulty", admissionContext),
                    RuleJson.RequireFloat(admission, "performanceWeight", admissionContext),
                    RuleJson.RequireFloat(admission, "knowledgeWeight", admissionContext),
                    RuleJson.RequireInt(admission, "retryWeeks", admissionContext)),
                new ScholarshipRules(
                    RuleJson.RequireFloat(scholarship, "minScore", scholarshipContext),
                    ChanceSpecReader.Read(scholarship["chance"], scholarshipContext.Property("chance")),
                    RuleJson.RequireFloat(scholarship, "keepPerformance", scholarshipContext)),
                loan));
        }
    }
}
