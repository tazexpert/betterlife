using System;
using System.Linq;
using BetterLife.Core.Economy;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Economy
{
    /// <summary>Tests for <see cref="EconomyService"/> and <see cref="EconomySettings"/>.</summary>
    [TestFixture]
    public class EconomyServiceTests
    {
        private static EconomySettings Settings => FinanceFixture.Content.Get<EconomySettings>();

        [Test]
        public void CreateInitial_PriceLevelFollowsHistoricalInflation()
        {
            EconomyState now = EconomyService.CreateInitial(Settings, 2026, new Pcg32(1UL, 1UL));
            EconomyState past = EconomyService.CreateInitial(Settings, 1950, new Pcg32(1UL, 1UL));

            Assert.That(now.PriceLevel, Is.EqualTo(1m));
            Assert.That((double)past.PriceLevel, Is.EqualTo(Math.Pow(1.035, -76)).Within(1e-6));
            Assert.That(past.Nominal(10000m), Is.EqualTo(Money.Round(10000m * past.PriceLevel)));
        }

        [Test]
        public void AdvanceYear_RaisesPrices_AndFollowsThePhaseChain()
        {
            var random = new Pcg32(5UL, 1UL);
            var economy = new EconomyService(Settings, EconomyService.CreateInitial(Settings, 2026, random), random);
            var seen = new System.Collections.Generic.List<string> { economy.State.PhaseId };

            for (int year = 0; year < 30; year++)
            {
                decimal before = economy.State.PriceLevel;
                float inflation = economy.State.AnnualInflation;
                string phase = economy.State.PhaseId;

                economy.AdvanceYear();

                Assert.That(economy.State.PriceLevel, Is.EqualTo(before * (1m + (decimal)inflation)));
                if (economy.State.PhaseId != phase)
                {
                    Assert.That(economy.State.PhaseId, Is.EqualTo(Settings.Phases.Get(phase).NextId));
                }
                EconomyPhaseDefinition current = economy.Phase;
                Assert.That(economy.State.YearsLeftInPhase, Is.InRange(1, current.MaxYears));
                Assert.That(economy.State.AnnualInflation, Is.InRange(current.Inflation.Min, current.Inflation.Max));
                seen.Add(economy.State.PhaseId);
            }

            Assert.That(seen.Distinct(), Is.EquivalentTo(new[] { "expansion", "recession" }));
        }

        [Test]
        public void RollMonth_IsDeterministicPerSeed()
        {
            MarketMonth Roll(ulong seed)
            {
                var random = new Pcg32(seed, 1UL);
                return new EconomyService(Settings, EconomyService.CreateInitial(Settings, 2026, random), random).RollMonth();
            }

            MarketMonth a = Roll(9UL);
            MarketMonth b = Roll(9UL);

            Assert.That(new[] { a.Stocks, a.Property, a.Crypto }, Is.EqualTo(new[] { b.Stocks, b.Property, b.Crypto }));
            Assert.That(a.Inflation, Is.GreaterThan(0m));
        }

        [Test]
        public void Settings_RejectUnknownNextPhase()
        {
            var phase = new EconomyPhaseDefinition("boom", "economy.boom.name", "bust", 1, 2, FloatRange.Fixed(0.02f), FloatRange.Fixed(0.02f),
                new MarketParameters(0.05f, 0.1f), new MarketParameters(0.03f, 0.05f), new MarketParameters(0.2f, 0.5f));

            Assert.Throws<ArgumentException>(() => new EconomySettings(2026, 0.035f, 1000m, new[] { phase }));
        }

        [Test]
        public void WealthCurve_InterpolatesAndClamps()
        {
            var curve = new WealthCurve(new[] { (0f, 30f), (6f, 45f), (-24f, 5f) });

            Assert.That(curve.ScoreFor(3f), Is.EqualTo(37.5f).Within(1e-4));
            Assert.That(curve.ScoreFor(-12f), Is.EqualTo(17.5f).Within(1e-4));
            Assert.That(curve.ScoreFor(-100f), Is.EqualTo(5f));
            Assert.That(curve.ScoreFor(100f), Is.EqualTo(45f));
            Assert.Throws<ArgumentException>(() => new WealthCurve(new[] { (0f, 30f) }));
        }
    }
}
