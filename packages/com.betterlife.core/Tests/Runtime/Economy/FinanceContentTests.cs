using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Economy;
using BetterLife.Core.Tests.Content;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Economy
{
    /// <summary>Tests for the economy and finance loaders.</summary>
    [TestFixture]
    public class FinanceContentTests
    {
        [Test]
        public void TestContent_LoadsEveryFinanceProduct()
        {
            IContentRepository content = FinanceFixture.Content;

            Assert.That(content.Get<EconomySettings>().Phases.All, Has.Count.EqualTo(2));
            Assert.That(content.Get<DefinitionCatalog<AssetDefinition>>().Get("siam_tech").Beta, Is.EqualTo(1.4f));
            Assert.That(content.Get<DefinitionCatalog<AssetDefinition>>().Get("index_fund").Beta, Is.EqualTo(1f));
            DebtProductDefinition mortgage = content.Get<DefinitionCatalog<DebtProductDefinition>>().Get("mortgage");
            Assert.That((mortgage.Kind, mortgage.CollateralClass, mortgage.AnnualRate, mortgage.RateSpread), Is.EqualTo((DebtKind.Mortgage, (AssetClass?)AssetClass.RealEstate, (float?)null, (float?)0.035f)));
            Assert.That(content.Get<DefinitionCatalog<LifestyleTierDefinition>>().Get("frugal").MonthlyStats, Has.Count.EqualTo(1));
            Assert.That(content.Get<FinanceSettings>().FamilySupport[5].IndependenceGift, Is.EqualTo(1000000m));
        }

        [TestCase("finance_settings.json", "'fallbackProduct': 'loan_shark'", "'fallbackProduct': 'bank_of_mom'", "unknown loan product 'bank_of_mom'")]
        [TestCase("finance_settings.json", "'defaultLifestyle': 'modest'", "'defaultLifestyle': 'palace'", "unknown lifestyle tier 'palace'")]
        [TestCase("flags.json", "'id': 'finance.bankrupt'", "'id': 'finance.broke'", "bool flag 'finance.bankrupt'")]
        [TestCase("debts.json", "'annualRate': 0.16, 'rateSpread': null", "'annualRate': 0.16, 'rateSpread': 0.1", "exactly one of a fixed rate")]
        [TestCase("assets.json", "'class': 'crypto'", "'class': 'tulips'", "'tulips' is not one of")]
        [TestCase("economy.json", "'next': 'recession'", "'next': 'depression'", "unknown phase 'depression'")]
        [TestCase("lifestyle_tiers.json", "'stat': 'mental.stress', 'amount': 1", "'stat': 'mental.calm', 'amount': 1", "unknown stat 'mental.calm'")]
        public void InvalidFinanceContent_IsReported(string file, string find, string replace, string message)
        {
            string original = Original(file);
            Assert.That(original, Does.Contain(find));
            InMemoryContentSource source = TestContent.Source().Add(file, original.Replace(find, replace));

            ContentLoadResult result = DefaultContent.CreatePipeline(source).Load();

            Assert.That(result.Errors.Select(e => e.Message), Has.Some.Contains(message), string.Join("\n", result.Errors));
        }

        private static string Original(string file)
        {
            switch (file)
            {
                case "finance_settings.json": return TestContent.FinanceSettings;
                case "flags.json": return TestContent.Flags;
                case "debts.json": return TestContent.DebtProducts;
                case "assets.json": return TestContent.Assets;
                case "economy.json": return TestContent.Economy;
                default: return TestContent.LifestyleTiers;
            }
        }
    }
}
