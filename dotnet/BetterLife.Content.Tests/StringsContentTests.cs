using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using BetterLife.Core.Achievements;
using BetterLife.Core.Astrology;
using BetterLife.Core.Careers;
using BetterLife.Core.Content;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.World;
using BetterLife.Core.Domain;
using BetterLife.Core.Economy;
using BetterLife.Core.Education;
using BetterLife.Core.Karma;
using BetterLife.Core.Life;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Localization;
using BetterLife.Core.Navigation;
using BetterLife.Core.Relationships;
using BetterLife.Core.Rules;
using BetterLife.Core.Scams;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Vices;
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

        [Test]
        public void ViewModelKeyConstants_AllHaveText()
        {
            // Every key constant of the view models (prefixes and number formats are not keys).
            var keyShape = new Regex("^[a-z][a-z0-9_]*(\\.[a-z][a-z0-9_]*)+$");
            IEnumerable<string> keys = typeof(DashboardViewModel).Assembly.GetTypes()
                .Where(t => t.Namespace == typeof(DashboardViewModel).Namespace)
                .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
                .Where(f => f.IsLiteral && f.FieldType == typeof(string) && !f.Name.EndsWith("Prefix"))
                .Select(f => (string)f.GetRawConstantValue())
                .Where(v => keyShape.IsMatch(v));
            AssertAllHaveText(keys.Distinct());
        }

        [Test]
        public void SubScreenTexts_AllHaveText()
        {
            var keys = new List<string>();
            keys.AddRange(_content.Get<DefinitionCatalog<AssetDefinition>>().All.Select(a => a.NameKey));
            keys.AddRange(_content.Get<DefinitionCatalog<DebtProductDefinition>>().All.Select(d => d.NameKey));
            keys.AddRange(_content.Get<DefinitionCatalog<LifestyleTierDefinition>>().All.Select(t => t.NameKey));
            keys.AddRange(_content.Get<EconomySettings>().Phases.All.Select(p => p.NameKey));
            var interactions = _content.Get<DefinitionCatalog<InteractionDefinition>>().All.ToList();
            keys.AddRange(interactions.Select(i => i.NameKey));
            keys.AddRange(interactions.SelectMany(i => new[] { i.Success.MemoryKey, i.Failure?.MemoryKey }).Where(k => k != null));
            keys.AddRange(new[] { RelationshipService.BreakupMemoryKey, RelationshipService.DivorceMemoryKey });
            keys.AddRange(_content.Get<DefinitionCatalog<EducationProgramDefinition>>().All.Select(p => p.NameKey));
            keys.AddRange(_content.Get<DefinitionCatalog<CareerTrackDefinition>>().All.Select(t => t.NameKey));
            keys.AddRange(_content.Get<DefinitionCatalog<DeathCauseDefinition>>().All.Select(c => c.NameKey));
            foreach (CountryDefinition country in _content.Get<CharacterCatalogs>().Countries.All)
            {
                keys.Add(country.NameKey);
                keys.AddRange(country.Cities.Select(c => c.NameKey));
            }

            // Every chart part a character can be born with.
            AstrologyTables tables = _content.Get<FortuneCalculator>().Tables;
            for (int year = 1950; year < 2010; year++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    foreach (int hour in new[] { 9, 21 })
                    {
                        AstrologyProfile chart = tables.ProfileFor(new BirthInfo(new CalendarDate(year, month, 1 + year % 28), hour, "thailand"));
                        keys.AddRange(new[] { chart.Zodiac.NameKey, chart.BirthDay.NameKey, chart.YearAnimal.NameKey, chart.Element.NameKey });
                    }
                }
            }

            keys.AddRange(RelationshipNames.Types.Keys.Select(n => PeopleViewModel.TypePrefix + n));
            keys.AddRange(new[] { "father", "mother", "son", "daughter", "husband", "wife", "older_brother", "older_sister", "younger_brother",
                "younger_sister", "twin_brother", "twin_sister" }.Select(r => PeopleViewModel.RolePrefix + r));
            keys.AddRange(System.Enum.GetValues(typeof(BetterLife.Core.Domain.Characters.BiologicalSex)).Cast<System.Enum>().Select(x => SessionText.EnumKey(PeopleViewModel.SexPrefix, x)));
            keys.AddRange(new[] { "partner", "spouse", "ex" }.Select(n => TimelineViewModel.BecamePrefix + n));
            keys.AddRange(FortuneAreas.All.Select(a => SessionText.EnumKey(AstrologyViewModel.AreaPrefix, a)));
            keys.AddRange(System.Enum.GetValues(typeof(FinanceLineCategory)).Cast<System.Enum>().Select(c => SessionText.EnumKey(FinanceViewModel.CategoryPrefix, c)));
            keys.AddRange(System.Enum.GetValues(typeof(JobEndReason)).Cast<System.Enum>().Select(r => SessionText.EnumKey(TimelineViewModel.JobEndPrefix, r)));
            keys.AddRange(Constants(typeof(InteractionQuote), "interaction.reason."));
            keys.AddRange(Constants(typeof(CompatibilityBreakdown), "compatibility."));
            keys.AddRange(Constants(typeof(ScamReportResult), "scam.report."));
            keys.AddRange(new[]
            {
                // Keys written as @key in the UXML of task 17.
                "finance.title", "finance.section.economy", "finance.section.holdings", "finance.section.debts", "finance.section.report",
                "finance.section.lifestyle", "finance.section.scams", "finance.lifestyle.locked", "finance.none", "people.title", "people.empty",
                "person.closeness", "person.compatibility", "person.memories", "person.actions", "more.title", "more.astrology", "more.timeline",
                "astrology.title", "astrology.chart", "astrology.karma", "timeline.title", "action.close",
            });
            AssertAllHaveText(keys.Where(k => k != null).Distinct());
        }

        [Test]
        public void PlayerActionTexts_AllHaveText()
        {
            var keys = new List<string>();
            foreach (ActivityDefinition activity in _content.Get<DefinitionCatalog<ActivityDefinition>>().All)
            {
                keys.Add(activity.NameKey);
                keys.Add(activity.DescKey);
            }
            keys.AddRange(_content.Get<DefinitionCatalog<QuitMethodDefinition>>().All.Select(m => m.NameKey));
            foreach (EducationProgramDefinition program in _content.Get<DefinitionCatalog<EducationProgramDefinition>>().All)
            {
                keys.AddRange(program.Institutions.All.Select(i => i.NameKey));
            }
            keys.AddRange(_content.Get<DefinitionCatalog<CareerTrackDefinition>>().All.SelectMany(t => t.Levels).Select(l => l.TitleKey));
            keys.AddRange(System.Enum.GetValues(typeof(AssetClass)).Cast<System.Enum>().Select(c => SessionText.EnumKey(InvestViewModel.ClassPrefix, c)));
            keys.AddRange(System.Enum.GetValues(typeof(TuitionFunding)).Cast<System.Enum>().Select(f => SessionText.EnumKey(WorkViewModel.PayerPrefix, f)));
            keys.AddRange(System.Enum.GetValues(typeof(BudgetIssueKind)).Cast<System.Enum>().Select(k => SessionText.EnumKey(LifePlanViewModel.IssuePrefix, k)));
            keys.AddRange(Constants(typeof(LoanQuote), "finance.loan."));
            keys.AddRange(Constants(typeof(AdmissionQuote), "education.admission."));
            keys.AddRange(Constants(typeof(HiringQuote), "career.apply."));
            keys.AddRange(Constants(typeof(QuitQuote), "vice.quit."));
            keys.AddRange(new[]
            {
                // Keys used by the task 17b screens.
                "life.title", "work.title", "work.section.education", "work.section.apply_school", "work.section.job", "work.section.apply_job",
                "work.drop_out", "work.quit_job", "invest.title", "invest.too_young", "invest.section.repay", "invest.section.sell",
                "invest.section.buy", "invest.section.borrow", "finance.open_invest",
            });
            AssertAllHaveText(keys.Where(k => k != null).Distinct());
        }

        [Test]
        public void LegacyTexts_AllHaveText()
        {
            var keys = new List<string>();
            foreach (AchievementDefinition achievement in _content.Get<DefinitionCatalog<AchievementDefinition>>().All)
            {
                keys.Add(achievement.NameKey);
                keys.Add(achievement.DescKey);
            }
            keys.AddRange(_content.Get<CharacterCatalogs>().Flags.All.Select(f => f.EulogyKey));
            keys.AddRange(_content.Get<DefinitionCatalog<DeathCauseDefinition>>().All.Select(c => c.EulogyKey));
            keys.AddRange(new[]
            {
                // Keys used by the task 18 screens.
                "summary.section.facts", "summary.section.achievements", "summary.section.next", "summary.no_heir", "summary.new_life",
                "summary.copy", "summary.copied", "tree.title", "achievements.title", "more.family_tree", "more.achievements", "more.summary",
                RuleTextKeys.CondGeneration,
            });
            AssertAllHaveText(keys.Where(k => k != null).Distinct());
        }

        [Test]
        public void EveryKeyInContentFiles_HasText()
        {
            // Every value of a "...Key" property in every content file (new content in later batches is covered too).
            var keys = new List<string>();
            foreach (string file in System.IO.Directory.GetFiles(RepoContent.ContentDir, "*.json", System.IO.SearchOption.AllDirectories))
            {
                if (file.Replace('\\', '/').Contains("/strings/"))
                {
                    continue;
                }
                var root = (Newtonsoft.Json.Linq.JContainer)Newtonsoft.Json.Linq.JToken.Parse(System.IO.File.ReadAllText(file));
                keys.AddRange(root.Descendants().OfType<Newtonsoft.Json.Linq.JProperty>()
                    .Where(p => p.Name.EndsWith("Key") && p.Value.Type == Newtonsoft.Json.Linq.JTokenType.String)
                    .Select(p => (string)p.Value));
            }
            Assert.That(keys, Is.Not.Empty);
            AssertAllHaveText(keys.Distinct());
        }

        [Test]
        public void PlayerFacingCodeKeys_HaveText()
        {
            // Text keys declared in Core code: rule texts, reasons and breakdown labels the player can see.
            string[] prefixes = { "rule.", "creation.issue.", "undo.reason.", "scam.targeting.", "vice.temptation.", "vice.use.", "source." };
            IEnumerable<string> keys = typeof(DashboardViewModel).Assembly.GetTypes()
                .SelectMany(t => t.GetFields(BindingFlags.Public | BindingFlags.Static))
                .Where(f => f.IsLiteral && f.FieldType == typeof(string))
                .Select(f => (string)f.GetRawConstantValue())
                .Where(v => prefixes.Any(v.StartsWith) && !v.EndsWith("."));
            AssertAllHaveText(keys.Distinct());
        }

        [Test]
        public void EveryTable_NamesItsLanguage()
        {
            foreach (string language in _tables.Languages)
            {
                Assert.That(_tables.Find(language).Find(SettingsViewModel.LanguageSelfKey), Is.Not.Null.And.Not.Empty, language);
            }
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
                List<string> missing = list.Where(k => _tables.Find(language).Find(k) == null).ToList();
                Assert.That(missing, Is.Empty, language + " is missing: " + string.Join(", ", missing));
            }
        }
    }
}
