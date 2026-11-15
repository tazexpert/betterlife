using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Events;
using BetterLife.Core.Karma;
using BetterLife.Core.Randomness;
using BetterLife.Core.Tests.Astrology;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Karma
{
    /// <summary>Tests for <see cref="KarmaLedger"/>, <see cref="KarmaService"/> and <see cref="KarmicDebtPhase"/>.</summary>
    [TestFixture]
    public class KarmaTests
    {
        private static readonly GameDate Now = GameDate.FromYearWeek(2026, 0);

        [Test]
        public void GoodAndBad_AreKeptSeparately_AndClamped()
        {
            var ledger = new KarmaLedger();
            ledger.AddGood(30f);
            ledger.AddBad(20f);
            ledger.AddGood(90f);

            Assert.That(ledger.Good, Is.EqualTo(100f));
            Assert.That(ledger.Bad, Is.EqualTo(20f), "good deeds never erase bad karma");
            Assert.Throws<ArgumentOutOfRangeException>(() => ledger.AddBad(-1f));
        }

        [Test]
        public void BadDeed_SchedulesDebt_WithinTheDelayWindow()
        {
            KarmaService service = TestAstrology.Karma();
            var character = TestCatalogs.NewCharacter();
            var random = new Pcg32(5UL, 5UL);

            KarmicDebt debt = service.RecordBadDeed(character, 20f, "event.theft", Now, random);

            Assert.That(character.Karma.Bad, Is.EqualTo(20f));
            Assert.That(debt.Amount, Is.EqualTo(10f));
            Assert.That(Now.WeeksUntil(debt.DueOn), Is.InRange(26, 260));
            Assert.That(debt.SourceKey, Is.EqualTo("event.theft"));
        }

        [Test]
        public void Merit_AddsGood_AndPaysOldestDebtsFirst_ButNeverReducesBad()
        {
            KarmaService service = TestAstrology.Karma();
            var character = TestCatalogs.NewCharacter();
            character.Karma.AddBad(30f);
            character.Karma.AddDebt(new KarmicDebt(4f, Now.AddWeeks(50), "event.late"));
            character.Karma.AddDebt(new KarmicDebt(10f, Now.AddWeeks(10), "event.early"));

            float paid = service.MakeMerit(character, 16f);

            Assert.That(paid, Is.EqualTo(8f), "50% of 16");
            Assert.That(character.Karma.Good, Is.EqualTo(16f));
            Assert.That(character.Karma.Bad, Is.EqualTo(30f));
            Assert.That(character.Karma.Debts.Select(d => (d.SourceKey, d.Amount)), Is.EqualTo(new[] { ("event.early", 2f), ("event.late", 4f) }));

            service.MakeMerit(character, 100f);
            Assert.That(character.Karma.Debts, Is.Empty);
        }

        [TestCase(0f, 0f, 0f)]
        [TestCase(50f, 0f, 5f)]
        [TestCase(0f, 50f, -5f)]
        [TestCase(100f, 0f, 10f)]
        [TestCase(0f, 100f, -10f)]
        [TestCase(80f, 30f, 5f)]
        public void ChancePoints_WeighGoodAgainstBad_WithinCap(float good, float bad, float expected)
        {
            var ledger = new KarmaLedger();
            ledger.AddGood(good);
            ledger.AddBad(bad);

            Assert.That(TestAstrology.Karma().ChancePoints(ledger), Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void DebtPhase_PublishesDueDebts_AndKeepsTheRest()
        {
            var character = TestCatalogs.NewCharacter();
            character.Karma.AddDebt(new KarmicDebt(3f, Now.AddWeeks(1), "event.a"));
            character.Karma.AddDebt(new KarmicDebt(5f, Now.AddWeeks(2), "event.b"));
            var bus = new EventBus();
            var due = new List<KarmicDebt>();
            bus.Subscribe<KarmicDebtDueEvent>(e => due.Add(e.Debt));
            var phase = new KarmicDebtPhase(bus);

            phase.Execute(new TickContext(Now.AddWeeks(1), character));

            Assert.That(due.Single().SourceKey, Is.EqualTo("event.a"));
            Assert.That(character.Karma.Debts.Single().SourceKey, Is.EqualTo("event.b"));
            Assert.That(phase.Order, Is.EqualTo(TickOrder.DelayedConsequences));
        }

        [Test]
        public void Settings_ValidateRanges()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new KarmaSettings(0.1f, 0.1f, 10f, 1.5f, 1, 2, 0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new KarmaSettings(0.1f, 0.1f, 10f, 0.5f, 10, 5, 0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new KarmicDebt(0f, Now, "event.x"));
        }
    }
}
