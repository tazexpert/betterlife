using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Time
{
    /// <summary>Tests for <see cref="TimeSystem"/>.</summary>
    [TestFixture]
    public class TimeSystemTests
    {
        private TimeFixture _f;
        private List<string> _log;

        [SetUp]
        public void SetUp()
        {
            _f = new TimeFixture();
            _log = new List<string>();
        }

        [Test]
        public void Phases_RunInOrder_EachWeek_ByCadence()
        {
            TimeSystem system = System(
                new RecordingPhase("yearly", 50, TickCadence.Yearly, _log),
                new RecordingPhase("weekly", 10, TickCadence.Weekly, _log),
                new RecordingPhase("monthly", 20, TickCadence.Monthly, _log));
            system.Clock.MoveTo(GameDate.FromYearWeek(2025, 50));

            system.AdvanceWeeks(_f.Character, 3);

            Assert.That(_log, Is.EqualTo(new[]
            {
                "weekly@2025-W51",
                "weekly@2026-W00", "monthly@2026-W00", "yearly@2026-W00",
                "weekly@2026-W01",
            }));
        }

        [Test]
        public void SkipLengths_ReachNextMonthOrYear()
        {
            TimeSystem system = System();

            Assert.That(system.WeeksFor(TimeSkip.Week), Is.EqualTo(1));
            Assert.That(system.WeeksFor(TimeSkip.Month), Is.EqualTo(5), "Jan has 5 game weeks");
            Assert.That(system.WeeksFor(TimeSkip.Year), Is.EqualTo(52));

            AdvanceResult result = system.Advance(_f.Character, TimeSkip.Month);
            Assert.That(result.To.IsMonthStart, Is.True);
            Assert.That(result.To.Month, Is.EqualTo(2));
            Assert.That(system.WeeksFor(TimeSkip.Month), Is.EqualTo(4), "Feb has 4 game weeks");
        }

        [Test]
        public void MajorInterrupt_StopsAfterThatWeekCompletes()
        {
            var interrupter = new InterruptingPhase(TimeFixture.Start.AddWeeks(3), Severity.Major);
            TimeSystem system = System(interrupter, new RecordingPhase("after", 999, TickCadence.Weekly, _log));

            AdvanceResult result = system.Advance(_f.Character, TimeSkip.Year);

            Assert.That(result.WeeksAdvanced, Is.EqualTo(3));
            Assert.That(result.WasInterrupted, Is.True);
            Assert.That(result.StoppedBy.SourceId, Is.EqualTo("test.event"));
            Assert.That(_log.Last(), Is.EqualTo("after@2026-W03"), "the rest of the week still ran");
            Assert.That(system.Clock.Now, Is.EqualTo(TimeFixture.Start.AddWeeks(3)));
        }

        [Test]
        public void MinorInterrupt_IsRecordedButDoesNotStop_AndThresholdIsConfigurable()
        {
            var interrupter = new InterruptingPhase(TimeFixture.Start.AddWeeks(2), Severity.Normal);
            TimeSystem system = System(interrupter);

            AdvanceResult result = system.AdvanceWeeks(_f.Character, 5);
            Assert.That(result.WeeksAdvanced, Is.EqualTo(5));
            Assert.That(result.Interrupts, Has.Count.EqualTo(1));
            Assert.That(result.WasInterrupted, Is.False);

            system.Policy.Threshold = Severity.Normal;
            interrupter.At = system.Clock.Now.AddWeeks(1);
            Assert.That(system.AdvanceWeeks(_f.Character, 5).WeeksAdvanced, Is.EqualTo(1));
        }

        [Test]
        public void InterruptInTheFinalWeek_CompletesTheSkip()
        {
            TimeSystem system = System(new InterruptingPhase(TimeFixture.Start.AddWeeks(1), Severity.Critical));

            AdvanceResult result = system.Advance(_f.Character, TimeSkip.Week);

            Assert.That(result.WasInterrupted, Is.False);
            Assert.That(result.Interrupts.Single().Severity, Is.EqualTo(Severity.Critical));
        }

        [Test]
        public void Events_ArePublishedPerWeek_AndPerSkip()
        {
            var weeks = new List<GameDate>();
            AdvanceResult published = null;
            _f.Bus.Subscribe<WeekCompletedEvent>(e => weeks.Add(e.Week));
            _f.Bus.Subscribe<TimeAdvancedEvent>(e => published = e.Result);
            TimeSystem system = System();

            AdvanceResult result = system.AdvanceWeeks(_f.Character, 3);

            Assert.That(weeks, Is.EqualTo(new[] { TimeFixture.Start.AddWeeks(1), TimeFixture.Start.AddWeeks(2), TimeFixture.Start.AddWeeks(3) }));
            Assert.That(published, Is.SameAs(result));
        }

        [Test]
        public void InvalidSetup_IsRejected()
        {
            Assert.Throws<ArgumentException>(() => System(new RecordingPhase("a", 1, TickCadence.Weekly, _log), new RecordingPhase("a", 2, TickCadence.Weekly, _log)));
            Assert.Throws<ArgumentOutOfRangeException>(() => System().AdvanceWeeks(_f.Character, 0));
            Assert.Throws<ArgumentNullException>(() => System().AdvanceWeeks(null, 1));
        }

        private TimeSystem System(params ITickPhase[] phases)
        {
            return new TimeSystem(_f.State.Clock, phases, _f.Bus, new InterruptPolicy());
        }

        private sealed class RecordingPhase : ITickPhase
        {
            private readonly List<string> _log;

            public RecordingPhase(string name, int order, TickCadence cadence, List<string> log)
            {
                Name = name;
                Order = order;
                Cadence = cadence;
                _log = log;
            }

            public string Name { get; }

            public int Order { get; }

            public TickCadence Cadence { get; }

            public void Execute(TickContext context) => _log.Add($"{Name}@{context.Week}");
        }

        private sealed class InterruptingPhase : ITickPhase
        {
            private readonly Severity _severity;

            public InterruptingPhase(GameDate at, Severity severity)
            {
                At = at;
                _severity = severity;
            }

            public GameDate At { get; set; }

            public string Name => "interrupter";

            public int Order => TickOrder.EventRoll;

            public TickCadence Cadence => TickCadence.Weekly;

            public void Execute(TickContext context)
            {
                if (context.Week == At)
                {
                    context.RequestInterrupt(_severity, "interrupt.test", "test.event");
                }
            }
        }
    }
}
