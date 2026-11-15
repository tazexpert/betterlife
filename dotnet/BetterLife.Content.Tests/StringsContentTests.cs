using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BetterLife.Core.Careers;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Economy;
using BetterLife.Core.Education;
using BetterLife.Core.Karma;
using BetterLife.Core.Life;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Relationships;
using BetterLife.Core.Rules;
using BetterLife.Core.Scams;
using BetterLife.Core.Vices;
using BetterLife.Core.Localization;
using BetterLife.Core.Navigation;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Checks the real string tables: every language has the same keys, and every key the shell uses exists.
    /// </summary>
    [TestFixture]
    public class StringsContentTests
    {
        private IContentRepository _content;
        private StringTables _tables;

        [OneTimeSetUp]
        public void Load()
        {
            _content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            _tables = _content.Get<StringTables>();
        }

        [Test]
        public void EveryLanguage_HasTheSameKeys()
        {
            StringTable english = _tables.Find("en");
            Assert.That(_tables.Languages, Is.EquivalentTo(new[] { "en", "th" }));
            foreach (string language in _tables.Languages)
            {
                Assert.That(_tables.Find(language).Keys, Is.EquivalentTo(english.Keys), $"{language} keys differ from en");
            }
        }

        [Test]
        public void ShellKeys_AllHaveText()
        {
            var keys = ShellTabs.All.Select(t => t.LabelKey)
                .Concat(new[] { TopBarViewModel.NameKey, TopBarViewModel.AgeKey, TopBarViewModel.DateKey })
                .Concat(Enumerable.Range(1, 12).Select(m => TopBarViewModel.MonthKeyPrefix + m))
                .Concat(new[] { MoneyFormatter.FullKey, MoneyFormatter.ThousandKey, MoneyFormatter.MillionKey, MoneyFormatter.BillionKey, MoneyFormatter.NegativeKey });

            foreach (string language in _tables.Languages)
            {
                Assert.That(keys.Where(k => _tables.Find(language).Find(k) == null), Is.Empty, language);
            }
        }

        [Test]
        public void ScreenKeys_AllHaveText()
        {
            var keys = new[]
            {
                DashboardViewModel.InboxKey, DashboardViewModel.AddictionKey, DashboardViewModel.QuittingKey,
                BreakdownViewModel.FormulaKey, BreakdownViewModel.RecentKey, BreakdownViewModel.EmptyKey, BreakdownViewModel.TermKey,
                InboxViewModel.RespondKey, InboxViewModel.DueNowKey, InboxViewModel.EmptyKey,
                EventDialogViewModel.ChanceKey, EventDialogViewModel.LockedKey, EventDialogViewModel.RollSuccessKey, EventDialogViewModel.RollFailureKey,
                // Keys written as @key in the UXML of task 16.
                "dashboard.hint", "dashboard.habits", "inbox.title", "inbox.decisions", "inbox.notices",
                "event.if_failed", "event.continue", "event.later", "breakdown.close",
            };
            AssertAllHaveText(keys);
        }

        [Test]
        public void GameTexts_AllHaveText()
        {
            var catalogs = _content.Get<CharacterCatalogs>();
            var events = _content.Get<DefinitionCatalog<LifeEventDefinition>>().All.ToList();
            IEnumerable<string> keys = catalogs.Stats.Definitions.Select(d => d.NameKey)
                .Concat(catalogs.Traits.All.Select(t => t.NameKey))
                .Concat(catalogs.Skills.All.Select(s => s.NameKey))
                .Concat(_content.Get<DefinitionCatalog<ViceDefinition>>().All.Select(v => v.NameKey))
                .Concat(_content.Get<DefinitionCatalog<ScamDefinition>>().All.Select(v => v.NameKey))
                .Concat(events.SelectMany(e => new[] { e.TitleKey, e.BodyKey }))
                .Concat(events.SelectMany(e => e.Choices).SelectMany(c => new[] { c.TextKey, c.Success.ResultKey, c.Failure?.ResultKey }))
                .Concat(Constants(typeof(RuleTextKeys), "rule.effect.")).Concat(Constants(typeof(KarmaTextKeys), "rule.effect."))
                .Concat(new[]
                {
                    AgingPhase.StageInterruptKey, DeathCheckPhase.DeathInterruptKey, FinanceService.TroubleInterruptKey,
                    CareerService.PromotedInterruptKey, CareerService.JobLostInterruptKey, CareerService.RetiredInterruptKey,
                    EducationService.GraduatedInterruptKey, EducationService.TroubleInterruptKey,
                    RelationshipService.ChildInterruptKey, RelationshipService.LeftInterruptKey, RelationshipService.DeathInterruptKey,
                    ViceService.RelapseInterruptKey, ViceService.DiscoveredInterruptKey, ViceService.QuitInterruptKey, ScamService.ScammedInterruptKey,
                });
            AssertAllHaveText(keys.Where(k => k != null).Distinct());
        }

        private static IEnumerable<string> Constants(System.Type type, string prefix)
        {
            return type.GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue())
                .Where(v => v.StartsWith(prefix));
        }

        private void AssertAllHaveText(IEnumerable<string> keys)
        {
            List<string> list = keys.ToList();
            foreach (string language in _tables.Languages)
            {
                Assert.That(list.Where(k => _tables.Find(language).Find(k) == null), Is.Empty, language);
            }
        }
    }
}
