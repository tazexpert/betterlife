using System.Linq;
using BetterLife.Core.Localization;
using BetterLife.Core.Relationships;
using BetterLife.Core.Saving;
using BetterLife.Core.Scams;
using BetterLife.Core.Sessions;
using BetterLife.Core.Tests.Dashboard;
using BetterLife.Core.Tests.Saving;
using BetterLife.Core.Time;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.SubScreens
{
    /// <summary>Tests for the Money, People, Astrology and Timeline view models and the new decisions.</summary>
    [TestFixture]
    public class SubScreenTests
    {
        private ILocalizer _keys;
        private MoneyFormatter _money;
        private UndoService _undo;
        private DecisionService _decisions;

        [SetUp]
        public void SetUp()
        {
            _keys = DashboardFixture.Keys();
            _money = new MoneyFormatter(_keys);
            _undo = new UndoService(new SaveSerializer(), new InMemorySaveStorage(), new SystemRealTimeClock());
            _decisions = new DecisionService(_undo, SaveFixture.Content.ContentVersion);
        }

        [Test]
        public void Finance_ShowsOverviewHoldingsReportAndLifestyles()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 20);
                life.Character.Wallet.Deposit(50000m);
                life.Finance.Buy("index_fund", 5000m);
                FinanceState state = FinanceViewModel.Build(life, _keys, _money);

                Assert.That(state.Summary.Select(r => r.Label), Is.EqualTo(new[]
                {
                    FinanceViewModel.CashKey, FinanceViewModel.InvestmentsKey, FinanceViewModel.DebtsKey, "finance.net_worth", FinanceViewModel.IncomeKey,
                }));
                Assert.That(state.Economy.Count, Is.EqualTo(3));
                Assert.That(state.Holdings.Single().Label, Is.EqualTo("asset.index_fund.name"));
                Assert.That(state.Report.Last().Label, Is.EqualTo(FinanceViewModel.NetKey), "a month has passed, so the report ends with the net change");
                Assert.That(state.ReportNote, Is.Empty);
                Assert.That(state.Lifestyles.Count(l => l.IsSelected), Is.EqualTo(1));
                Assert.That(state.CanChooseLifestyle, Is.True, "20 years old");

                string other = state.Lifestyles.First(l => !l.IsSelected).Id;
                _decisions.ChooseLifestyle(life, other);
                Assert.That(FinanceViewModel.Build(life, _keys, _money).Lifestyles.Single(l => l.IsSelected).Id, Is.EqualTo(other));
                Assert.That(_undo.HasSnapshot, Is.True);
            }
        }

        [Test]
        public void Finance_ChildCannotChooseLifestyle_AndNewLifeHasNoReport()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                FinanceState state = FinanceViewModel.Build(life, _keys, _money);
                Assert.That(state.CanChooseLifestyle, Is.False);
                Assert.That(state.ReportNote, Is.EqualTo(FinanceViewModel.NoReportKey));
                Assert.That(state.Holdings, Is.Empty);
                Assert.That(state.Scams, Is.Empty);
            }
        }

        [Test]
        public void ScamIncidents_CanBeReportedOnce_WithinTheWindow()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 20);
                GameDate now = life.TimeState.Clock.Now;
                ScamIncident recent = life.Character.Scams.AddIncident("phone_gang", now, 8000m, 20);
                ScamIncident old = life.Character.Scams.AddIncident("phone_gang", now.AddWeeks(-200), 3000m, 20);

                FinanceState state = FinanceViewModel.Build(life, _keys, _money);
                ScamIncidentRow first = state.Scams[0];
                Assert.That(first.IncidentId, Is.EqualTo(recent.Id), "newest first");
                Assert.That(first.CanReport, Is.True);
                Assert.That(first.ReportText, Is.EqualTo(FinanceViewModel.ScamReportKey));
                Assert.That(FinanceViewModel.ReportChance(life, recent, now), Is.GreaterThan(0f));
                Assert.That(state.Scams[1].CanReport, Is.False, "too late");

                ActionResultState result = FinanceViewModel.BuildReportResult(life, _decisions.ReportScam(life, recent.Id), _keys, _money);
                Assert.That(result.Title, Is.EqualTo(FinanceViewModel.ReportTitleKey));
                Assert.That(result.Lines.Last(), Is.EqualTo(result.Succeeded ? FinanceViewModel.ReportRecoveredKey : FinanceViewModel.ReportNothingKey));
                Assert.That(FinanceViewModel.Build(life, _keys, _money).Scams[0].CanReport, Is.False, "once only");

                ActionResultState again = FinanceViewModel.BuildReportResult(life, _decisions.ReportScam(life, recent.Id), _keys, _money);
                Assert.That(again.Succeeded, Is.False);
                Assert.That(again.Lines, Is.EqualTo(new[] { ScamReportResult.ReasonReported }));
            }
        }

        [Test]
        public void People_AreGrouped_AndAPersonCanBeVisited()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 20);
                var groups = PeopleViewModel.Build(life, _keys);

                Assert.That(groups.First().Title, Is.EqualTo(PeopleViewModel.FamilyKey), "parents come with the life");
                Assert.That(groups.SelectMany(g => g.People).Count(), Is.EqualTo(life.Character.Relationships.All.Count(r => life.Character.Relationships.Npc(r.NpcId) != null)));
                foreach (PeopleGroup group in groups)
                {
                    Assert.That(group.People.Where(p => p.IsAlive).Select(p => p.Value), Is.Ordered.Descending);
                }

                Relationship parent = life.Character.Relationships.Living(RelationshipType.Parent).First();
                PersonState person = PeopleViewModel.BuildPerson(life, parent.NpcId, _keys, _money);
                Assert.That(person.Compatibility.First().Label, Is.EqualTo(PeopleViewModel.CompatibilityKey));
                Assert.That(person.Interactions.Select(i => i.Id), Is.EquivalentTo(life.Relationships.Options(parent.NpcId).Select(o => o.Interaction.Id)));
                Assert.That(PeopleViewModel.BuildPerson(life, 99999, _keys, _money), Is.Null);

                InteractionOption chat = person.Interactions.FirstOrDefault(i => i.IsAvailable);
                Assume.That(chat, Is.Not.Null, "some interaction is available with a parent");
                InteractionResult result = _decisions.Interact(life, parent.NpcId, chat.Id);
                ActionResultState shown = PeopleViewModel.BuildResult(life, chat.Id, result, _keys, _money);
                Assert.That(result.Performed, Is.True);
                Assert.That(shown.Title, Is.EqualTo("interaction." + chat.Id + ".name"));
                Assert.That(_undo.HasSnapshot, Is.True);
            }
        }

        [Test]
        public void Astrology_ShowsChartFiveFortunesAndKarma()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                AstrologyState state = AstrologyViewModel.Build(life, _keys);

                Assert.That(state.Profile.Count, Is.EqualTo(4));
                Assert.That(state.Fortunes.Select(f => f.Area), Is.EquivalentTo(new[]
                {
                    "fortune.area.finance", "fortune.area.love", "fortune.area.health", "fortune.area.career", "fortune.area.study",
                }));
                Assert.That(state.Fortunes.All(f => f.Parts.Count == 6 && f.Value >= 0f && f.Value <= 100f), Is.True);
                Assert.That(state.Karma.Select(k => k.Label), Is.EqualTo(new[]
                {
                    AstrologyViewModel.GoodKey, AstrologyViewModel.BadKey, AstrologyViewModel.PointsKey, AstrologyViewModel.DebtKey,
                }));
            }
        }

        [Test]
        public void Timeline_StartsWithBirth_AndIsNewestFirst()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 25);
                GameDate now = life.TimeState.Clock.Now;
                life.Character.Scams.AddIncident("phone_gang", now, 8000m, 20);
                var timeline = TimelineViewModel.Build(life, _keys, _money);

                Assert.That(timeline.Select(e => e.Week), Is.Ordered.Descending);
                Assert.That(timeline.Last().Kind, Is.EqualTo(TimelineEntryKind.Birth));
                Assert.That(timeline.Last().AgeText, Is.EqualTo("topbar.age"));
                Assert.That(timeline.Count(e => e.Kind == TimelineEntryKind.Education), Is.EqualTo(life.Character.Education.Credentials.Count));
                Assert.That(timeline.Count(e => e.Kind == TimelineEntryKind.Event), Is.EqualTo(life.Events.State.History.Count));
                Assert.That(timeline.First().Kind, Is.EqualTo(TimelineEntryKind.Scam));

                life.Character.Die("accident", now.AddWeeks(1));
                Assert.That(TimelineViewModel.Build(life, _keys, _money).First().Kind, Is.EqualTo(TimelineEntryKind.Death));
                Assert.That(TimelineViewModel.Build(life, _keys, _money).Count, Is.LessThanOrEqualTo(TimelineViewModel.MaxEntries));
            }
        }
    }
}
