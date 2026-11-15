using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules.Serialization;
using BetterLife.Core.Tests.Astrology;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Luck
{
    /// <summary>Tests for <see cref="LuckCalculator"/> and <see cref="ChanceSpecReader"/>.</summary>
    [TestFixture]
    public class LuckCalculatorTests
    {
        private static readonly GameDate Now = GameDate.FromYearWeek(2026, 10);

        private Character _aries;

        [SetUp]
        public void SetUp()
        {
            _aries = AstrologyTests.Born(new CalendarDate(2000, 4, 20), 12);
        }

        [Test]
        public void FinalChance_SumsEveryTerm()
        {
            _aries.Stats.SetStored(StatIds.Iq, 70f);
            _aries.Stats.SetStored(StatIds.Knowledge, 70f);
            _aries.Karma.AddGood(30f);
            var spec = new ChanceSpec(40f, new[] { new ChanceStatTerm(StatIds.Intelligence, 0.5f) }, FortuneArea.Career, true);

            ChanceBreakdown breakdown = Calculator(new FixedBonus(3f)).Explain(spec, _aries, Now);

            Assert.That(breakdown.Base, Is.EqualTo(40f));
            Assert.That(breakdown.Stat, Is.EqualTo(10f).Within(0.0001f), "(70 − 50) × 0.5");
            Assert.That(breakdown.Luck, Is.EqualTo(2f).Within(0.0001f), "(60 − 50) × 0.2");
            Assert.That(breakdown.Karma, Is.EqualTo(3f).Within(0.0001f));
            Assert.That(breakdown.Item, Is.EqualTo(3f));
            Assert.That(breakdown.Final, Is.EqualTo(58f).Within(0.0001f));
            Assert.That(breakdown.IsClamped, Is.False);
        }

        [Test]
        public void FinalChance_IsClampedBetween5And95()
        {
            var sure = new ChanceSpec(100f, null, null, false);
            var hopeless = new ChanceSpec(0f, new[] { new ChanceStatTerm(StatIds.Stress, 1f) }, null, false);
            _aries.Stats.SetStored(StatIds.Stress, 0f);

            ChanceBreakdown high = Calculator().Explain(sure, _aries, Now);
            ChanceBreakdown low = Calculator().Explain(hopeless, _aries, Now);

            Assert.That(high.Final, Is.EqualTo(95f));
            Assert.That(high.IsClamped, Is.True);
            Assert.That(low.Raw, Is.EqualTo(-50f));
            Assert.That(low.Final, Is.EqualTo(5f));
        }

        [Test]
        public void ItemBonus_IsCappedAtFivePercent_AndNeverNegative()
        {
            var spec = new ChanceSpec(50f, null, null, false);

            Assert.That(Calculator(new FixedBonus(4f), new FixedBonus(4f)).Explain(spec, _aries, Now).Item, Is.EqualTo(5f));
            Assert.That(Calculator(new FixedBonus(-3f)).Explain(spec, _aries, Now).Item, Is.EqualTo(0f));
        }

        [Test]
        public void KarmaAndFortune_ApplyOnlyWhenRequested()
        {
            _aries.Karma.AddBad(100f);

            ChanceBreakdown plain = Calculator().Explain(new ChanceSpec(50f, null, null, false), _aries, Now);
            ChanceBreakdown karmic = Calculator().Explain(new ChanceSpec(50f, null, null, true), _aries, Now);

            Assert.That(plain.Karma, Is.EqualTo(0f));
            Assert.That(plain.Luck, Is.EqualTo(0f));
            Assert.That(karmic.Karma, Is.EqualTo(-10f));
        }

        [Test]
        public void Roll_IsDeterministic_AndMatchesTheFinalChanceOverManyRolls()
        {
            var spec = new ChanceSpec(30f, null, null, false);
            LuckCalculator calculator = Calculator();
            var a = new Pcg32(9UL, 9UL);
            var b = new Pcg32(9UL, 9UL);

            List<ChanceRoll> rolls = Enumerable.Range(0, 10000).Select(_ => calculator.Roll(spec, _aries, Now, a)).ToList();

            Assert.That(calculator.Roll(spec, _aries, Now, b).Roll, Is.EqualTo(rolls[0].Roll));
            Assert.That(rolls.Count(r => r.Success) / 10000.0, Is.EqualTo(0.3).Within(0.02));
            Assert.That(rolls.Select(r => r.Roll), Is.All.InRange(0f, 100f));
        }

        [Test]
        public void Reader_ParsesAndRoundTrips()
        {
            var catalogs = TestCatalogs.All();
            JToken json = JToken.Parse("{ 'base': 40, 'stats': [ { 'stat': 'intelligence', 'weight': 0.3 } ], 'fortune': 'study', 'karma': false }");

            ChanceSpec spec = ChanceSpecReader.Read(json, new RuleReadContext(catalogs));

            Assert.That(spec.BasePercent, Is.EqualTo(40f));
            Assert.That(spec.StatTerms.Single().StatId, Is.EqualTo(StatIds.Intelligence));
            Assert.That(spec.FortuneArea, Is.EqualTo(FortuneArea.Study));
            Assert.That(spec.UseKarma, Is.False);
            Assert.That(JToken.DeepEquals(ChanceSpecReader.Write(spec), json), Is.True);
            Assert.That(ChanceSpecReader.Read(JToken.Parse("{ 'base': 10 }"), new RuleReadContext(catalogs)).UseKarma, Is.True);
        }

        [TestCase("{ 'base': 140 }", "$", "within 0-100")]
        [TestCase("{ 'base': 40, 'fortune': 'gambling' }", "$.fortune", "not one of")]
        [TestCase("{ 'base': 40, 'stats': [ { 'stat': 'charm', 'weight': 1 } ] }", "$.stats[0].stat", "unknown stat")]
        [TestCase("{ 'base': 40, 'bonus': 5 }", "$.bonus", "unknown property")]
        public void Reader_RejectsInvalidSpecs(string json, string path, string message)
        {
            var ex = Assert.Throws<RuleParseException>(() => Wrap(() => ChanceSpecReader.Read(JToken.Parse(json), new RuleReadContext(TestCatalogs.All()))));

            Assert.That(ex.Path, Is.EqualTo(path));
            Assert.That(ex.Problem, Does.Contain(message));
        }

        [Test]
        public void Settings_ValidateRanges()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new LuckSettings(60f, 40f, 5f, 50f, 0.2f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new LuckSettings(5f, 95f, -1f, 50f, 0.2f));
        }

        private static ChanceSpec Wrap(Func<ChanceSpec> read)
        {
            try
            {
                return read();
            }
            catch (ArgumentException ex)
            {
                throw new RuleParseException("$", ex.Message, ex);
            }
        }

        private static LuckCalculator Calculator(params IChanceBonusSource[] sources)
        {
            return new LuckCalculator(TestAstrology.Luck(), new FortuneCalculator(TestAstrology.Tables()), TestAstrology.Karma(), sources);
        }

        private sealed class FixedBonus : IChanceBonusSource
        {
            private readonly float _bonus;

            public FixedBonus(float bonus)
            {
                _bonus = bonus;
            }

            public float BonusFor(Character character, ChanceSpec spec, GameDate now) => _bonus;
        }
    }
}
