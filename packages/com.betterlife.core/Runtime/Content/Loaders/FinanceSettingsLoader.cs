using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content.Loaders
{
    /// <summary>
    /// Loads content/finance_settings.json into <see cref="FinanceSettings"/> and checks the lifestyles, loan products,
    /// stats and flags the finance code relies on.
    /// </summary>
    public sealed class FinanceSettingsLoader : SingleDocumentLoader
    {
        private static readonly string[] Dependencies =
        {
            ContentTypes.Stats, ContentTypes.Flags, ContentTypes.LifestyleTiers, ContentTypes.DebtProducts,
        };

        private static readonly StatId[] RequiredStats = { StatIds.Wealth, StatIds.CreditScore, StatIds.Stress };

        private static readonly string[] RequiredFlags = { FinanceService.LoanSharkFlag, FinanceService.BankruptFlag };

        /// <inheritdoc />
        public override string TypeName => ContentTypes.FinanceSettings;

        /// <inheritdoc />
        public override IReadOnlyList<string> DependsOn => Dependencies;

        /// <inheritdoc />
        protected override void Read(JObject root, RuleReadContext context, ContentLoadContext content)
        {
            RuleJson.RequireOnly(root, context, "independenceAge", "allowanceStartAge", "studentSupportUntilAge", "defaultLifestyle", "referenceLifestyle",
                "familySupport", "fallbackProduct", "defaultAfterMissed", "forcedSaleDiscount", "creditScore", "bankruptcy",
                "debtStress", "wealthCurve", ContentReading.SchemaProperty);
            RequireCodeContent(context, content);

            var lifestyles = content.Require<DefinitionCatalog<LifestyleTierDefinition>>();
            var products = content.Require<DefinitionCatalog<DebtProductDefinition>>();
            string defaultLifestyle = RequireKnown(root, "defaultLifestyle", context, lifestyles.Contains, "lifestyle tier");
            string referenceLifestyle = RequireKnown(root, "referenceLifestyle", context, lifestyles.Contains, "lifestyle tier");
            string fallback = RequireKnown(root, "fallbackProduct", context, products.Contains, "loan product");

            var (support, supportContext) = RuleJson.RequireArrayProperty(root, "familySupport", context);
            List<FamilySupportLevel> levels = support.Select((token, i) =>
            {
                RuleReadContext c = supportContext.Index(i);
                JObject level = RuleJson.RequireObject(token, c);
                RuleJson.RequireOnly(level, c, "incomeLevel", "monthlyAllowance", "independenceGift");
                return new FamilySupportLevel(RuleJson.RequireInt(level, "incomeLevel", c),
                    RuleJson.RequireDecimal(level, "monthlyAllowance", c), RuleJson.RequireDecimal(level, "independenceGift", c));
            }).ToList();

            var (credit, creditContext) = Section(root, "creditScore", context,
                "onTimeGain", "missedLoss", "defaultLoss", "foreclosureLoss", "bankruptcyScore", "highUtilization", "utilizationLoss");
            var (bankruptcy, bankruptcyContext) = Section(root, "bankruptcy", context, "netWorthBelow", "flagWeeks");
            var (stress, stressContext) = Section(root, "debtStress", context, "perYearOfCosts", "max", "informalExtra");
            var (curve, curveContext) = RuleJson.RequireArrayProperty(root, "wealthCurve", context);
            List<(float Months, float Score)> points = curve.Select((token, i) =>
            {
                RuleReadContext c = curveContext.Index(i);
                JObject point = RuleJson.RequireObject(token, c);
                RuleJson.RequireOnly(point, c, "months", "score");
                return (RuleJson.RequireFloat(point, "months", c), RuleJson.RequireFloat(point, "score", c));
            }).ToList();

            content.Add(new FinanceSettings(
                RuleJson.RequireInt(root, "independenceAge", context),
                RuleJson.RequireInt(root, "allowanceStartAge", context),
                RuleJson.RequireInt(root, "studentSupportUntilAge", context),
                defaultLifestyle,
                referenceLifestyle,
                levels,
                fallback,
                RuleJson.RequireInt(root, "defaultAfterMissed", context),
                RuleJson.RequireDecimal(root, "forcedSaleDiscount", context),
                new CreditScoreRules(
                    RuleJson.RequireFloat(credit, "onTimeGain", creditContext),
                    RuleJson.RequireFloat(credit, "missedLoss", creditContext),
                    RuleJson.RequireFloat(credit, "defaultLoss", creditContext),
                    RuleJson.RequireFloat(credit, "foreclosureLoss", creditContext),
                    RuleJson.RequireFloat(credit, "bankruptcyScore", creditContext),
                    RuleJson.RequireFloat(credit, "highUtilization", creditContext),
                    RuleJson.RequireFloat(credit, "utilizationLoss", creditContext)),
                RuleJson.RequireDecimal(bankruptcy, "netWorthBelow", bankruptcyContext),
                RuleJson.RequireInt(bankruptcy, "flagWeeks", bankruptcyContext),
                RuleJson.RequireFloat(stress, "perYearOfCosts", stressContext),
                RuleJson.RequireFloat(stress, "max", stressContext),
                RuleJson.RequireFloat(stress, "informalExtra", stressContext),
                new WealthCurve(points)));
        }

        private static void RequireCodeContent(RuleReadContext context, ContentLoadContext content)
        {
            StatCatalog stats = content.Require<StatCatalog>();
            foreach (StatId stat in RequiredStats)
            {
                if (!stats.Contains(stat) || stats.IsDerived(stat))
                {
                    throw context.Error($"finance needs the stored stat '{stat}'.");
                }
            }
            var flags = content.Require<DefinitionCatalog<FlagDefinition>>();
            foreach (string flag in RequiredFlags)
            {
                if (!flags.Contains(flag) || flags.Get(flag).Kind != FlagKind.Bool)
                {
                    throw context.Error($"finance needs the bool flag '{flag}'.");
                }
            }
        }

        private static string RequireKnown(JObject root, string name, RuleReadContext context, System.Func<string, bool> exists, string what)
        {
            string id = RuleJson.RequireId(root, name, context);
            if (!exists(id))
            {
                throw context.Property(name).Error($"unknown {what} '{id}'.");
            }
            return id;
        }

        private static (JObject Section, RuleReadContext Context) Section(JObject parent, string name, RuleReadContext context, params string[] allowed)
        {
            var (token, sectionContext) = RuleJson.RequireProperty(parent, name, context);
            JObject section = RuleJson.RequireObject(token, sectionContext);
            RuleJson.RequireOnly(section, sectionContext, allowed);
            return (section, sectionContext);
        }
    }
}
