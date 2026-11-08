using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Karma;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Persistence;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Core.Tests.LifeEvents
{
    /// <summary>Tests for <see cref="EventSystem"/>.</summary>
    [TestFixture]
    public class EventSystemTests
    {
        private static readonly StatId Fitness = new StatId("health.fitness");

        private const string ChoiceEvent = @"{ 'id': 'test.choice', 'titleKey': 'event.test.choice.title', 'bodyKey': 'event.test.choice.body',
            'category': 'test', 'trigger': 'random', 'severity': 'major', 'tone': 'neutral', 'cooldownWeeks': 1000, 'defaultChoice': 'rich',
            'choices': [
              { 'id': 'safe', 'textKey': 'c.safe', 'success': { 'resultKey': 'r.safe', 'effects': [ { 'type': 'stat', 'stat': 'health.fitness', 'amount': 2 } ] } },
              { 'id': 'risky', 'textKey': 'c.risky', 'chance': { 'base': 50, 'karma': false },
                'success': { 'resultKey': 'r.win', 'effects': [ { 'type': 'stat', 'stat': 'health.fitness', 'amount': 10 } ] },
                'failure': { 'resultKey': 'r.lose', 'effects': [ { 'type': 'stat', 'stat': 'health.fitness', 'amount': -10 } ] } },
              { 'id': 'rich', 'textKey': 'c.rich', 'condition': { 'type': 'cash', 'op': '>=', 'amount': 1000 },
                'success': { 'resultKey': 'r.rich', 'effects': [ { 'type': 'cash', 'amount': -1000 } ] } },
              { 'id': 'secret', 'textKey': 'c.secret', 'hideWhenLocked': true, 'condition': { 'type': 'flag', 'flag': 'career.fired_once' },
                'success': { 'resultKey': 'r.secret' } },
              { 'id': 'plan', 'textKey': 'c.plan',
                'success': { 'resultKey': 'r.plan', 'effects': [ { 'type': 'event.schedule', 'event': 'test.follow_up', 'delayWeeks': 3 } ] } } ] }";

        private static readonly string FollowUp = EventFixture.Simple("test.follow_up",
            ", 'onFire': [ { 'type': 'stat', 'stat': 'health.fitness', 'amount': 1 } ]", "scheduled", "positive", "normal");

        [Test]
        public void Announcement_AppliesImmediately_AndGoesToHistory()
        {
            var f = new EventFixture(EventFixture.Events(EventFixture.Simple("test.announce",
                ", 'onFire': [ { 'type': 'stat', 'stat': 'health.fitness', 'amount': -5 }, { 'type': 'cash', 'amount': -100 } ]", severity: "normal")));
            float before = f.Character.Stats.GetValue(Fitness);

            TickContext tick = f.RunWeek();

            Assert.That(f.Character.Stats.GetValue(Fitness), Is.EqualTo(before - 5f));
            Assert.That(f.System.Inbox, Is.Empty);
            Assert.That(f.State.History.Single().EventId, Is.EqualTo("test.announce"));
            Assert.That(f.Fired.Single().Origin, Is.EqualTo(EventOrigin.Random));
            Assert.That(tick.Interrupts.Single().Severity, Is.EqualTo(Severity.Normal));
            Assert.That(f.Shortfalls.Single().Amount, Is.EqualTo(100m), "messages from effects are published");
            Assert.That(f.Character.ChangeLog.Entries.First().SourceKey, Is.EqualTo("event.test.announce.title"));
        }

        [Test]
        public void ChoiceEvent_WaitsInInbox_AndViewsExplainEachChoice()
        {
            var f = new EventFixture(EventFixture.Events(ChoiceEvent, FollowUp));

            TickContext tick = f.RunWeek();
            PendingEvent pending = f.System.Inbox.Single();
            IReadOnlyList<ChoiceView> views = f.System.GetChoices(f.Character, pending.InstanceId);

            Assert.That(tick.Interrupts.Single().Severity, Is.EqualTo(Severity.Major));
            Assert.That(pending.Deadline, Is.EqualTo(pending.FiredOn.AddWeeks(4)));
            Assert.That(views.Select(v => v.Choice.Id), Is.EqualTo(new[] { "safe", "risky", "rich", "secret", "plan" }));
            Assert.That(views.Single(v => v.Choice.Id == "rich").IsAvailable, Is.False);
            Assert.That(views.Single(v => v.Choice.Id == "rich").IsVisible, Is.True, "locked but shown");
            Assert.That(views.Single(v => v.Choice.Id == "rich").Requirement.Passed, Is.False);
            Assert.That(views.Single(v => v.Choice.Id == "secret").IsVisible, Is.False, "hidden while locked");
            Assert.That(views.Single(v => v.Choice.Id == "risky").Chance.Final, Is.EqualTo(50f));
            Assert.That(views.Single(v => v.Choice.Id == "risky").FailurePreview.Single().Args["value"], Is.EqualTo("-10"));
            Assert.That(views.Single(v => v.Choice.Id == "safe").Chance, Is.Null);
        }

        [Test]
        public void Resolve_AppliesOutcome_AndRecordsHistory()
        {
            var f = new EventFixture(EventFixture.Events(ChoiceEvent, FollowUp));
            f.RunWeek();
            int id = f.System.Inbox.Single().InstanceId;
            float before = f.Character.Stats.GetValue(Fitness);
            EventResolution published = null;
            f.Bus.Subscribe<LifeEventResolvedEvent>(e => published = e.Resolution);

            EventResolution resolution = f.System.Resolve(f.Character, id, "risky");

            Assert.That(published, Is.SameAs(resolution));
            Assert.That(resolution.Roll, Is.Not.Null);
            float expected = resolution.Succeeded ? 10f : -10f;
            Assert.That(f.Character.Stats.GetValue(Fitness) - before, Is.EqualTo(expected));
            Assert.That(resolution.Outcome.ResultKey, Is.EqualTo(resolution.Succeeded ? "r.win" : "r.lose"));
            Assert.That(f.System.Inbox, Is.Empty);
            EventHistoryEntry history = f.State.History.Single();
            Assert.That((history.ChoiceId, history.Succeeded), Is.EqualTo(("risky", resolution.Succeeded)));
        }

        [Test]
        public void Resolve_RejectsLockedUnknownAndMissing()
        {
            var f = new EventFixture(EventFixture.Events(ChoiceEvent, FollowUp));
            f.RunWeek();
            int id = f.System.Inbox.Single().InstanceId;

            Assert.Throws<InvalidOperationException>(() => f.System.Resolve(f.Character, id, "rich"));
            Assert.Throws<ArgumentException>(() => f.System.Resolve(f.Character, id, "fly"));
            Assert.Throws<ArgumentException>(() => f.System.Resolve(f.Character, 999, "safe"));
        }

        [Test]
        public void DelayedConsequence_FiresOnItsWeek()
        {
            var f = new EventFixture(EventFixture.Events(ChoiceEvent, FollowUp));
            f.RunWeek();
            f.System.Resolve(f.Character, f.System.Inbox.Single().InstanceId, "plan");

            Assert.That(f.State.Scheduled.Single().DueOn, Is.EqualTo(EventFixture.Start.AddWeeks(4)));
            f.RunWeek();
            f.RunWeek();
            Assert.That(f.Fired.Select(e => e.Definition.Id), Is.EqualTo(new[] { "test.choice" }));
            f.RunWeek();
            Assert.That(f.Fired.Last().Definition.Id, Is.EqualTo("test.follow_up"));
            Assert.That(f.Fired.Last().Origin, Is.EqualTo(EventOrigin.Scheduled));
        }

        [Test]
        public void ScheduledEvent_IsDroppedWhenItsConditionNoLongerHolds()
        {
            string conditional = EventFixture.Simple("test.conditional", ", 'condition': { 'type': 'flag', 'flag': 'career.fired_once' }", "scheduled");
            var f = new EventFixture(EventFixture.Events(conditional), weeklyChance: 0f);
            f.State.Schedule(new ScheduledEvent("test.conditional", EventFixture.Start.AddWeeks(1)));

            f.RunWeek();

            Assert.That(f.Fired, Is.Empty);
            Assert.That(f.State.Scheduled, Is.Empty);
        }

        [Test]
        public void Overdue_UsesDefaultChoice_OrFirstAvailable()
        {
            var f = new EventFixture(EventFixture.Events(ChoiceEvent, FollowUp));
            f.RunWeek();
            float before = f.Character.Stats.GetValue(Fitness);
            EventResolution auto = null;
            f.Bus.Subscribe<LifeEventResolvedEvent>(e => auto = e.Resolution);

            for (int i = 0; i < 4; i++)
            {
                f.RunWeek();
            }

            Assert.That(auto.WasAutomatic, Is.True);
            Assert.That(auto.Choice.Id, Is.EqualTo("safe"), "default 'rich' is locked without cash, so the first available is used");
            Assert.That(f.Character.Stats.GetValue(Fitness), Is.EqualTo(before + 2f));
            Assert.That(f.System.Inbox, Is.Empty);
        }

        [Test]
        public void Weights_Modifiers_Cooldown_AndLimits_ShapeTheDraw()
        {
            var f = new EventFixture(EventFixture.Events(
                EventFixture.Simple("test.common", ", 'weight': { 'base': 3 }"),
                EventFixture.Simple("test.rare", ", 'weight': { 'base': 1, 'modifiers': [ { 'condition': { 'type': 'flag', 'flag': 'career.fired_once' }, 'multiplier': 0 } ] }"),
                EventFixture.Simple("test.once", ", 'weight': { 'base': 1 }, 'maxOccurrences': 1"),
                EventFixture.Simple("test.cooling", ", 'weight': { 'base': 1 }, 'cooldownWeeks': 5000"),
                EventFixture.Simple("test.never", ", 'condition': { 'type': 'age', 'op': '>', 'value': 99 }")));

            for (int i = 0; i < 2000; i++)
            {
                f.RunWeek();
            }
            var counts = f.Fired.GroupBy(e => e.Definition.Id).ToDictionary(g => g.Key, g => g.Count());

            Assert.That(counts["test.once"], Is.EqualTo(1));
            Assert.That(counts["test.cooling"], Is.EqualTo(1));
            Assert.That(counts.ContainsKey("test.never"), Is.False);
            Assert.That(counts["test.common"] / (double)counts["test.rare"], Is.EqualTo(3.0).Within(0.4));

            f.Character.Flags.Set("career.fired_once", f.Clock.Now);
            int rareBefore = counts["test.rare"];
            for (int i = 0; i < 200; i++)
            {
                f.RunWeek();
            }
            Assert.That(f.Fired.Count(e => e.Definition.Id == "test.rare"), Is.EqualTo(rareBefore), "multiplier 0 removes it");
        }

        [Test]
        public void WeeklyChance_ControlsHowOftenRandomEventsHappen()
        {
            var f = new EventFixture(EventFixture.Events(EventFixture.Simple("test.common")), weeklyChance: 0.15f);

            for (int i = 0; i < 4000; i++)
            {
                f.RunWeek();
            }

            Assert.That(f.Fired.Count / 4000.0, Is.EqualTo(0.15).Within(0.02));
        }

        [Test]
        public void AntiFrustration_SoftensBadNewsAfterAStreak()
        {
            var f = new EventFixture(EventFixture.Events(
                EventFixture.Simple("test.bad", tone: "negative"),
                EventFixture.Simple("test.good", tone: "positive")));
            int bad = 0;
            const int Trials = 3000;

            for (int i = 0; i < Trials; i++)
            {
                GameDate recent = f.Clock.Now;
                f.State.Restore(null, null, null, new[] { new ToneEntry(recent, EventTone.Negative), new ToneEntry(recent, EventTone.Negative) }, null, 1);
                f.RunWeek();
                bad += f.Fired.Last().Definition.Id == "test.bad" ? 1 : 0;
            }

            Assert.That(bad / (double)Trials, Is.EqualTo(0.4 / 1.4).Within(0.03));
        }

        [Test]
        public void KarmicDebt_PicksAMisfortuneMatchingItsSize()
        {
            var f = new EventFixture(EventFixture.Events(
                EventFixture.Simple("test.small_luck", ", 'debtAmount': { 'min': 0, 'max': 5 }", "karmic_debt", "negative"),
                EventFixture.Simple("test.big_luck", ", 'debtAmount': { 'min': 5.01, 'max': 100 }", "karmic_debt", "negative", "major")), weeklyChance: 0f);

            f.Bus.Publish(new KarmicDebtDueEvent(new KarmicDebt(3f, f.Clock.Now, "event.test")));
            f.RunWeek();
            f.Bus.Publish(new KarmicDebtDueEvent(new KarmicDebt(12f, f.Clock.Now, "event.test")));
            TickContext tick = f.RunWeek();
            f.RunWeek();

            Assert.That(f.Fired.Select(e => (e.Definition.Id, e.Origin)), Is.EqualTo(new[]
            {
                ("test.small_luck", EventOrigin.KarmicDebt), ("test.big_luck", EventOrigin.KarmicDebt),
            }));
            Assert.That(tick.Interrupts.Single().Severity, Is.EqualTo(Severity.Major));
        }

        [Test]
        public void PlayerChoices_DoNotChangeWhichEventsComeLater()
        {
            List<string> Run(string choice)
            {
                var f = new EventFixture(EventFixture.Events(ChoiceEvent.Replace("'cooldownWeeks': 1000", "'cooldownWeeks': 0"), FollowUp,
                    EventFixture.Simple("test.common")), weeklyChance: 0.5f, seed: 99UL);
                for (int i = 0; i < 100; i++)
                {
                    f.RunWeek();
                    foreach (PendingEvent pending in f.System.Inbox.ToList())
                    {
                        f.System.Resolve(f.Character, pending.InstanceId, choice);
                    }
                }
                return f.Fired.Where(e => e.Origin == EventOrigin.Random).Select(e => $"{e.Week}:{e.Definition.Id}").ToList();
            }

            Assert.That(Run("risky"), Is.EqualTo(Run("safe")));
        }

        [Test]
        public void MajorEvent_StopsATimeSkip()
        {
            var f = new EventFixture(EventFixture.Events(ChoiceEvent, FollowUp));
            var system = new TimeSystem(f.Clock, new ITickPhase[] { new EventRollPhase(f.System) }, f.Bus, new InterruptPolicy());

            AdvanceResult result = system.Advance(f.Character, TimeSkip.Year);

            Assert.That(result.WeeksAdvanced, Is.EqualTo(1));
            Assert.That(result.StoppedBy.SourceId, Is.EqualTo("test.choice"));
        }

        [Test]
        public void State_RoundTripsThroughSave_AndSkipsRemovedEvents()
        {
            var f = new EventFixture(EventFixture.Events(ChoiceEvent, FollowUp));
            f.RunWeek();
            f.State.Schedule(new ScheduledEvent("test.follow_up", f.Clock.Now.AddWeeks(5)));

            EventStateDto dto = EventStateDtoMapper.ToDto(f.State);
            dto.Scheduled.Add(new ScheduledEventDto { EventId = "removed.event", DueWeek = 1 });
            var events = f.Content.Get<DefinitionCatalog<LifeEventDefinition>>();
            EventStateLoadResult loaded = EventStateDtoMapper.FromDto(dto, events);

            Assert.That(loaded.Warnings.Single(), Does.Contain("removed.event"));
            Assert.That(loaded.State.Inbox.Single().EventId, Is.EqualTo("test.choice"));
            Assert.That(loaded.State.Scheduled.Single().EventId, Is.EqualTo("test.follow_up"));
            Assert.That(loaded.State.FindRecord("test.choice").Occurrences, Is.EqualTo(1));
            Assert.That(loaded.State.Tones.Single().Tone, Is.EqualTo(EventTone.Neutral));
            Assert.That(loaded.State.NextInstanceId, Is.EqualTo(f.State.NextInstanceId));
        }

        [Test]
        public void Dispose_StopsListening()
        {
            var f = new EventFixture(EventFixture.Events(FollowUp), weeklyChance: 0f);
            f.System.Dispose();

            f.Bus.Publish(new ScheduleEventRequest("test.follow_up", f.Clock.Now.AddWeeks(1)));

            Assert.That(f.State.Scheduled, Is.Empty);
        }
    }
}
