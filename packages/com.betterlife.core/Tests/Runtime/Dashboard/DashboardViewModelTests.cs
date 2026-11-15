using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Localization;
using BetterLife.Core.Notices;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Dashboard
{
    /// <summary>Tests for <see cref="DashboardViewModel"/>, <see cref="BreakdownViewModel"/> and <see cref="InboxViewModel"/>.</summary>
    [TestFixture]
    public class DashboardViewModelTests
    {
        [Test]
        public void Dashboard_HasTheFiveCoreCards_WithHistory()
        {
            ILocalizer keys = DashboardFixture.Keys();
            using (LifeSession life = DashboardFixture.NewLife())
            {
                for (int i = 0; i < 12; i++)
                {
                    life.Advance(TimeSkip.Week);
                }
                DashboardState state = DashboardViewModel.Build(life, keys);

                Assert.That(state.Cards.Select(c => c.StatId), Is.EqualTo(life.Character.Catalogs.Stats.CoreStatIds));
                Assert.That(state.Cards.Select(c => c.StatId), Is.EquivalentTo(new[] { StatIds.Health, StatIds.Mental, StatIds.Intelligence, StatIds.Wealth, StatIds.Relationships }));
                StatCardState health = state.Cards[0];
                Assert.That(health.Name, Is.EqualTo("stat.health.name"));
                Assert.That(health.Value, Is.EqualTo(life.Character.Stats.GetValue(StatIds.Health)));
                Assert.That(health.Sparkline.Count, Is.EqualTo(life.Character.History.GetSamples(StatIds.Health).Count));
                Assert.That(health.DeltaText == string.Empty, Is.EqualTo(health.Direction == TrendDirection.Flat));
                Assert.That(state.Habits, Is.Empty, "a baby has no habits");
                Assert.That(state.PendingDecisions, Is.EqualTo(life.Events.Inbox.Count));
            }
        }

        [Test]
        public void Breakdown_ExplainsCoreFormula_AndRecentChanges()
        {
            ILocalizer keys = DashboardFixture.Keys();
            using (LifeSession life = DashboardFixture.NewLife())
            {
                life.Advance(TimeSkip.Week);
                BreakdownState health = BreakdownViewModel.Build(life, StatIds.Health, keys);
                Assert.That(health.Heading, Is.EqualTo(BreakdownViewModel.FormulaKey));
                Assert.That(health.Lines, Is.Not.Empty);
                Assert.That(health.Lines.Select(l => System.Math.Abs(l.Contribution)), Is.Ordered.Descending);
                Assert.That(health.Lines.All(l => l.Detail == BreakdownViewModel.TermKey), Is.True, "formula lines show input × weight");

                BreakdownState fitness = BreakdownViewModel.Build(life, StatIds.Fitness, keys);
                Assert.That(fitness.Heading, Is.EqualTo(BreakdownViewModel.RecentKey));
                Assert.That(fitness.Lines.All(l => l.Detail == string.Empty), Is.True);
            }
        }

        [Test]
        public void Inbox_ListsDecisionsWithDeadline_AndNews()
        {
            ILocalizer keys = DashboardFixture.Keys();
            using (LifeSession life = DashboardFixture.NewLife())
            {
                var feed = new NoticeFeed();
                AdvanceResult result = life.Advance(TimeSkip.Year);
                feed.Add(result, life.Events.Inbox);

                Assert.That(result.WasInterrupted, Is.True, "the major choice event stops time");
                InboxState inbox = InboxViewModel.Build(life, feed, keys);
                InboxItemState decision = inbox.Decisions.Single();
                Assert.That(decision.Kind, Is.EqualTo(InboxItemKind.Decision));
                Assert.That(decision.Title, Is.EqualTo("event.test.offer.title"));
                Assert.That(decision.Subtitle, Is.EqualTo(InboxViewModel.RespondKey));
                Assert.That(decision.InstanceId, Is.EqualTo(NoticeFeed.StoppingDecision(result, life.Events.Inbox)));
                Assert.That(inbox.Notices.Any(n => n.Title == "event.test.offer.title"), Is.False, "decisions are not repeated as news");
                Assert.That(inbox.IsEmpty, Is.False);

                Assert.That(InboxViewModel.Build(life, null, keys).Notices, Is.Empty);
            }
        }
    }
}
