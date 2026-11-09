using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Relationships;
using BetterLife.Core.Vices;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Vices
{
    /// <summary>Tests for <see cref="ViceService"/>.</summary>
    [TestFixture]
    public class ViceServiceTests
    {
        [Test]
        public void Temptation_MultipliesEveryFactor()
        {
            var calm = new ViceFixture();
            var pressured = new ViceFixture();
            pressured.Character.Traits.Add("anxious", pressured.Clock.Now);
            pressured.ChangeStat(StatIds.Stress, 80f);
            pressured.Add(RelationshipType.Friend, 40f, "anxious");
            pressured.Add(RelationshipType.Friend, 10f, "anxious");
            pressured.Bus.Publish(new BetterLife.Core.Time.Ticking.Phases.TimeAllocationAppliedEvent(pressured.Clock.Now,
                new Dictionary<string, int> { ["free_time"] = 50 }, new StatChangeResult[0], 0m));

            TemptationBreakdown baseline = calm.Service.Temptation("drinking");
            TemptationBreakdown heavy = pressured.Service.Temptation("drinking");

            var lines = heavy.Multipliers.ToDictionary(l => l.Key, l => l.Multiplier);
            Assert.That(baseline.WeeklyChance, Is.EqualTo(0.1f * (1f + (20f - 50f) / 50f) * 1.5f * 0.3f).Within(1e-5),
                "base × low stress × adult stage × first try");
            Assert.That(lines[TemptationBreakdown.StressKey], Is.EqualTo(1f + (100f - 50f) / 50f).Within(1e-4));
            Assert.That(lines[TemptationBreakdown.BadCompanyKey], Is.EqualTo(1.25f), "only the friend above 30 counts");
            Assert.That(lines[TemptationBreakdown.LazinessKey], Is.EqualTo(1f + 20 * 0.02f).Within(1e-4));
            Assert.That(lines[TemptationBreakdown.TraitsKey], Is.EqualTo(1.5f));
            Assert.That(heavy.WeeklyChance, Is.GreaterThan(baseline.WeeklyChance * 5f));
        }

        [Test]
        public void Resist_SubtractsAddiction_AndAddsSupportAndQuitting()
        {
            var f = new ViceFixture();
            f.Character.Wallet.Deposit(1000m);
            f.Service.Indulge("drinking");
            f.State("drinking").ChangeAddiction(40f);
            f.Add(RelationshipType.Parent, 60f);
            f.Add(RelationshipType.Colleague, 90f);
            f.Service.StartQuit("drinking", "cold_turkey");

            ResistQuote quote = f.Service.Resist("drinking");

            Assert.That(quote.AddictionAdjustment, Is.EqualTo(-0.4f * 50f).Within(1e-3));
            Assert.That(quote.SupportAdjustment, Is.EqualTo(5f), "colleagues do not count as support");
            Assert.That(quote.QuitAdjustment, Is.EqualTo(5f));
            Assert.That(quote.Chance.Final, Is.EqualTo(50f - 20f + 5f + 5f).Within(1f));
        }

        [Test]
        public void Indulging_GivesReliefThatFadesWithTolerance_AndBuildsAddiction()
        {
            var f = new ViceFixture();
            f.Character.Wallet.Deposit(1000m);
            var used = new List<ViceUsedEvent>();
            f.Bus.Subscribe<ViceUsedEvent>(used.Add);
            float happiness = f.Stat(StatIds.Happiness);

            f.Service.Indulge("drinking");
            float first = f.Stat(StatIds.Happiness) - happiness;
            f.Service.Indulge("drinking");
            float second = f.Stat(StatIds.Happiness) - happiness - first;

            ViceState state = f.State("drinking");
            Assert.That(first, Is.EqualTo(3f).Within(1e-3));
            Assert.That(second, Is.EqualTo(3f * 0.8f).Within(1e-3), "tolerance 20 cuts relief by a fifth");
            Assert.That(state.Addiction, Is.EqualTo(10f + 10f * 0.9f).Within(1e-3));
            Assert.That((state.Uses, state.Tolerance, f.Character.Wallet.Cash), Is.EqualTo((2, 40f, 400m)));
            Assert.That(used.Select(u => u.Result.Voluntary), Is.All.True);
        }

        [Test]
        public void Uses_AreRefused_WhenTooYoungDisabledOrBroke_ButAddictsGoIntoDebt()
        {
            var child = new ViceFixture(2010);
            var off = new ViceFixture();
            var broke = new ViceFixture();
            var addict = new ViceFixture();
            off.Service.ContentEnabled = false;
            addict.Character.Vices.Get("drinking").ChangeAddiction(50f);
            var shortfalls = new List<CashShortfallEvent>();
            addict.Bus.Subscribe<CashShortfallEvent>(shortfalls.Add);

            Assert.That(child.Service.Indulge("drinking").ReasonKeys, Is.EqualTo(new[] { ViceUseResult.ReasonTooYoung }));
            Assert.That(off.Service.Indulge("drinking").ReasonKeys, Is.EqualTo(new[] { ViceUseResult.ReasonDisabled }));
            Assert.That(broke.Service.Indulge("drinking").ReasonKeys, Is.EqualTo(new[] { ViceUseResult.ReasonCash }));
            Assert.That(addict.Service.Indulge("drinking").Performed, Is.True);
            Assert.That(shortfalls.Single().Amount, Is.EqualTo(300m));
        }

        [Test]
        public void CleanWeeks_LowerAddiction_AndBringWithdrawal()
        {
            var f = new ViceFixture();
            f.Character.Wallet.Deposit(10000m);
            f.Character.Vices.Get("betting").ChangeAddiction(45f);
            f.Service.Indulge("betting");
            float addiction = f.State("betting").Addiction;
            float stress = f.Stat(StatIds.Stress);

            f.Week();

            Assert.That(f.State("betting").Addiction, Is.EqualTo(addiction - 0.5f).Within(1e-3));
            float withdrawal = f.Character.ChangeLog.Entries.Where(e => e.SourceKey == ViceService.WithdrawalKey && e.StatId == StatIds.Stress).Sum(e => e.Delta);
            Assert.That(withdrawal, Is.EqualTo(4f * (addiction - 0.5f) / 100f).Within(1e-3));
            Assert.That(f.Stat(StatIds.Stress), Is.GreaterThan(stress));
        }

        [Test]
        public void ActivityHours_AboveTheFreeAmount_CountAsUses()
        {
            var f = new ViceFixture(2010);

            f.Week(new Dictionary<string, int> { ["free_time"] = 99 });
            Assert.That(f.State("gaming"), Is.Null);
            f.Week(new Dictionary<string, int> { ["free_time"] = 125 });

            Assert.That(f.State("gaming").Uses, Is.EqualTo(2));
        }

        [Test]
        public void Temptation_LeadsToLapsesOrResistance_AndResistingBuildsWillpower()
        {
            var f = new ViceFixture();
            f.Character.Wallet.Deposit(1000000m);
            f.ChangeStat(StatIds.Stress, 80f);
            f.Character.Vices.Get("drinking").ChangeAddiction(60f);
            var lapses = new List<ViceUsedEvent>();
            var resisted = new List<TemptationResistedEvent>();
            f.Bus.Subscribe<ViceUsedEvent>(lapses.Add);
            f.Bus.Subscribe<TemptationResistedEvent>(resisted.Add);
            float willpower = f.Stat(StatIds.Willpower);

            for (int i = 0; i < 52; i++)
            {
                f.ChangeStat(StatIds.Stress, 100f);
                f.Week();
            }

            Assert.That(lapses, Is.Not.Empty);
            Assert.That(lapses.Select(l => l.Result.Voluntary), Is.All.False);
            Assert.That(resisted, Is.Not.Empty);
            Assert.That(f.Stat(StatIds.Willpower), Is.EqualTo(willpower + resisted.Count * 0.1f).Within(1e-3));
        }

        [Test]
        public void Gambling_HasBeginnersLuck_ThenLosesOnAverage_AndCannotBeUndone()
        {
            var f = new ViceFixture();
            f.Character.Wallet.Deposit(10000000m);
            var bets = new List<GambleResultEvent>();
            f.Bus.Subscribe<GambleResultEvent>(bets.Add);

            for (int i = 0; i < 400; i++)
            {
                f.Service.Indulge("betting");
            }

            ViceState state = f.State("betting");
            Assert.That(bets.Take(3).Count(b => b.Won), Is.GreaterThanOrEqualTo(2), "beginner's luck");
            Assert.That(bets.First().Stake, Is.EqualTo(1000m));
            Assert.That(bets.Last().Stake, Is.GreaterThan(1000m), "stakes grow with addiction");
            Assert.That(state.NetWinnings, Is.LessThan(0m), "the house wins");
            Assert.That(state.Bets, Is.EqualTo(400));
            Assert.That(f.Character.Flags.GetCount(ViceService.GamblingWinsFlag), Is.EqualTo(bets.Count(b => b.Won)));
            Assert.That(f.Character.Vices.LastIrreversibleWeek, Is.EqualTo(f.Clock.Now));
            Assert.That(f.Service.Indulge("betting").RiskKeys, Is.EqualTo(new[] { "vice.risk.seen" }));
        }

        [Test]
        public void Family_FindOut_OnlyWhenFamilyIsAround()
        {
            var alone = new ViceFixture();
            var withMum = new ViceFixture();
            Relationship mum = withMum.Add(RelationshipType.Parent, 60f);
            var found = new List<ViceDiscoveredEvent>();
            withMum.Bus.Subscribe<ViceDiscoveredEvent>(found.Add);
            foreach (ViceFixture f in new[] { alone, withMum })
            {
                f.Character.Vices.Get("drinking").ChangeAddiction(40f);
                f.Character.ApplyStatChange(new StatChange(StatIds.Willpower, 50f, "test.source"), f.Clock.Now);
                f.Week();
            }

            Assert.That(alone.State("drinking").Discovered, Is.False);
            Assert.That(withMum.State("drinking").Discovered, Is.True);
            Assert.That(found.Single().ViceId, Is.EqualTo("drinking"));
            Assert.That(mum.Value, Is.EqualTo(50f).Within(2f));
        }

        [Test]
        public void QuitQuote_ListsEveryReason()
        {
            var f = new ViceFixture();
            f.Character.Wallet.Deposit(1000m);

            Assert.That(f.Service.QuoteQuit("drinking", "cold_turkey").ReasonKeys, Is.EquivalentTo(new[] { QuitQuote.ReasonNotUsing }));
            f.Service.Indulge("drinking");
            Assert.That(f.Service.QuoteQuit("drinking", "rehab").ReasonKeys, Is.EquivalentTo(new[] { QuitQuote.ReasonCash }));
            Assert.That(f.Service.QuoteQuit("betting", "rehab").ReasonKeys, Does.Contain(QuitQuote.ReasonMethod));
            f.Service.StartQuit("drinking", "cold_turkey");
            Assert.That(f.Service.QuoteQuit("drinking", "cold_turkey").ReasonKeys, Is.EquivalentTo(new[] { QuitQuote.ReasonAlreadyQuitting }));
        }

        [Test]
        public void StayingClean_UntilTheEnd_Succeeds()
        {
            var f = new ViceFixture();
            f.Character.Wallet.Deposit(1000m);
            f.Service.Indulge("drinking");
            f.Service.StartQuit("drinking", "cold_turkey");
            f.Service.ContentEnabled = false; // no temptation: the character stays clean
            float willpower = f.Stat(StatIds.Willpower);
            var successes = new List<QuitSucceededEvent>();
            f.Bus.Subscribe<QuitSucceededEvent>(successes.Add);

            for (int i = 0; i <= 4; i++)
            {
                f.Week();
            }

            ViceState state = f.State("drinking");
            Assert.That((state.Quits, state.Quitting, state.Tolerance), Is.EqualTo((1, (QuitAttempt)null, 0f)));
            Assert.That(successes.Single().MethodId, Is.EqualTo("cold_turkey"));
            Assert.That(f.Stat(StatIds.Willpower), Is.EqualTo(willpower + 3f).Within(1e-3));
        }

        [Test]
        public void UsingWhileQuitting_IsARelapse()
        {
            var f = new ViceFixture();
            f.Character.Wallet.Deposit(1000m);
            f.Service.Indulge("drinking");
            f.Service.StartQuit("drinking", "cold_turkey");
            var relapses = new List<RelapseEvent>();
            f.Bus.Subscribe<RelapseEvent>(relapses.Add);

            ViceUseResult result = f.Service.Indulge("drinking");

            Assert.That(result.Relapsed, Is.True);
            Assert.That(f.State("drinking").Relapses, Is.EqualTo(1));
            Assert.That(f.State("drinking").Quitting, Is.Null);
            Assert.That(relapses.Single().MethodId, Is.EqualTo("cold_turkey"));
        }

        [Test]
        public void HabitCards_ShowOnlyOnceHooked_OrWhileQuitting()
        {
            var f = new ViceFixture();
            f.Character.Vices.Get("drinking").ChangeAddiction(5f);
            f.Character.Vices.Get("betting").ChangeAddiction(15f);

            Assert.That(f.Service.VisibleHabits().Select(s => s.ViceId), Is.EqualTo(new[] { "betting" }));
            Assert.That(f.Settings.HelplineKey, Is.EqualTo("vice.helpline"));
        }
    }
}
