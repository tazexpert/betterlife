using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Careers;
using BetterLife.Core.Economy;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Careers
{
    /// <summary>Tests for <see cref="CareerService"/> and the career income sources.</summary>
    [TestFixture]
    public class CareerServiceTests
    {
        [Test]
        public void Quote_ListsEveryReason()
        {
            var child = new SchoolWorkFixture(2010);
            var adult = new SchoolWorkFixture(2030);
            var senior = new SchoolWorkFixture(2061);
            adult.GiveJob("courier");

            Assert.That(child.Career.Quote("clerk").ReasonKeys, Is.EquivalentTo(new[] { HiringQuote.ReasonTooYoung, HiringQuote.ReasonRequirement }));
            Assert.That(adult.Career.Quote("courier").ReasonKeys, Is.EquivalentTo(new[] { HiringQuote.ReasonSameJob }));
            Assert.That(senior.Career.Quote("courier").ReasonKeys, Is.EquivalentTo(new[] { HiringQuote.ReasonTooOld }));
            Assert.That(adult.Career.Quote("clerk").StartingSalary, Is.EqualTo(20000m));
        }

        [Test]
        public void EducationPrestige_RaisesHiringOdds()
        {
            var top = new SchoolWorkFixture(2030);
            var open = new SchoolWorkFixture(2030);
            top.Graduate("bachelor", "bachelor.top", 70f);
            open.Graduate("bachelor", "bachelor.open", 70f);

            HiringQuote a = top.Career.Quote("clerk");
            HiringQuote b = open.Career.Quote("clerk");

            Assert.That(a.Eligible && b.Eligible, Is.True);
            Assert.That(a.PrestigeAdjustment, Is.EqualTo((90f - 50f) * 0.2f).Within(1e-4));
            Assert.That(a.Chance.Final, Is.GreaterThan(b.Chance.Final));
        }

        [Test]
        public void Applying_HiresAtAboutTheQuotedRate_ReservesHours_AndQuitsTheOldJob()
        {
            int hired = 0;
            float chance = 0f;
            for (ulong seed = 1; seed <= 200; seed++)
            {
                var f = new SchoolWorkFixture(2030, seed: seed);
                f.GiveJob("clerk");
                f.Graduate("bachelor", "bachelor.open", 70f);
                chance = f.Career.Quote("courier").Chance.Final;
                HiringResult result = f.Career.Apply("courier");
                if (!result.Hired)
                {
                    Assert.That(f.Career.Quote("courier").ReasonKeys, Does.Contain(HiringQuote.ReasonRetryWait));
                    continue;
                }
                hired++;
                Assert.That(f.Character.Career.Current.TrackId, Is.EqualTo("courier"));
                Assert.That(f.Character.Career.History.Single().Reason, Is.EqualTo(JobEndReason.Quit));
                Assert.That(f.Budget.GetHours("work"), Is.EqualTo(30));
            }

            Assert.That(hired / 200f * 100f, Is.EqualTo(chance).Within(10f));
        }

        [Test]
        public void WeeklyWork_DrivesPerformance_Boss_AndJobStress()
        {
            var worker = new SchoolWorkFixture(2030);
            var slacker = new SchoolWorkFixture(2030);
            worker.GiveJob("clerk");
            slacker.GiveJob("clerk");
            for (int i = 0; i < 30; i++)
            {
                worker.Week(work: 40);
                slacker.Week(work: 0);
            }

            Job good = worker.Character.Career.Current;
            Job bad = slacker.Character.Career.Current;
            Assert.That(good.Performance, Is.GreaterThan(bad.Performance + 20f));
            Assert.That(good.BossSatisfaction, Is.GreaterThan(bad.BossSatisfaction));
            Assert.That(worker.Character.ChangeLog.Entries.Where(e => e.SourceKey == CareerService.JobStressKey).Sum(e => e.Delta), Is.GreaterThan(0f));
            Assert.That(worker.Character.Skills.GetLevel("cooking"), Is.GreaterThan(0f));
        }

        [Test]
        public void UnhappyBoss_FiresAfterThreeMonths_AndReleasesTheHours()
        {
            var f = new SchoolWorkFixture(2030);
            f.GiveJob("clerk", boss: 5f);
            var ended = new List<JobEndedEvent>();
            f.Bus.Subscribe<JobEndedEvent>(ended.Add);

            f.Month();
            f.Month();
            Assert.That(f.Character.Career.IsEmployed, Is.True);
            f.Month();

            Assert.That(ended.Single().Job.Reason, Is.EqualTo(JobEndReason.Fired));
            Assert.That(f.Character.Flags.IsSet(CareerService.FiredFlag), Is.True);
            Assert.That(f.Budget.GetHours("work"), Is.EqualTo(0));
            Assert.That(f.LastTick.Interrupts, Is.Not.Empty);
        }

        [Test]
        public void Reviews_PromoteStrongPerformers_AndRescheduleTheRest()
        {
            int promoted = 0;
            for (ulong seed = 1; seed <= 40; seed++)
            {
                var f = new SchoolWorkFixture(2030, seed: seed);
                f.GiveJob("clerk", performance: 95f, boss: 90f, review: f.Clock.Now);
                f.Month();
                if (f.Character.Career.Current.LevelIndex == 1)
                {
                    promoted++;
                    Assert.That(f.Character.Career.Current.NextReviewOn, Is.Null, "manager is the top level");
                }
            }
            var weak = new SchoolWorkFixture(2030);
            Job job = weak.GiveJob("clerk", performance: 40f, boss: 90f, review: weak.Clock.Now);
            weak.Month();

            Assert.That(promoted, Is.GreaterThan(30));
            Assert.That(job.LevelIndex, Is.EqualTo(0));
            Assert.That(job.NextReviewOn, Is.EqualTo(weak.Clock.Now.AddWeeks(52)));
        }

        [Test]
        public void Retirement_EndsTheJob_AndPaysAPension()
        {
            var f = new SchoolWorkFixture(2061);
            f.GiveJob("clerk", level: 1);

            f.Month();

            Assert.That(f.Character.Career.IsRetired, Is.True);
            Assert.That(f.Character.Career.History.Single().Reason, Is.EqualTo(JobEndReason.Retired));
            Assert.That(f.Character.Career.PensionMonthly, Is.EqualTo(20000m));
            IncomeItem pension = new PensionIncomeSource().MonthlyIncome(f.Character, f.Clock.Now).Single();
            Assert.That((pension.SourceKey, pension.Amount, pension.Taxable), Is.EqualTo((PensionIncomeSource.SourceKey, 20000m, true)));
        }

        [Test]
        public void CareerRisk_AppliesItsEffects_AndEndsTheJob()
        {
            var f = new SchoolWorkFixture(2030, seed: 9UL);
            f.GiveJob("courier");
            f.Character.Wallet.Deposit(10000m);
            var risks = new List<CareerRiskEvent>();
            f.Bus.Subscribe<CareerRiskEvent>(risks.Add);

            for (int month = 0; month < 300 && f.Character.Career.IsEmployed; month++)
            {
                f.Month();
                f.Clock.MoveTo(f.Clock.Now.AddWeeks(4));
            }

            Assert.That(risks.Single().ReasonKey, Is.EqualTo("career.risk.arrested"));
            Assert.That(f.Character.Wallet.Cash, Is.EqualTo(5000m));
            Assert.That(f.Character.Career.History.Single().Reason, Is.EqualTo(JobEndReason.Risk));
        }

        [Test]
        public void Layoffs_FollowTheEconomyPhase()
        {
            var f = new SchoolWorkFixture(2030);

            Assert.That(f.CareerSettings.LayoffChance("recession"), Is.EqualTo(0.01f));
            Assert.That(f.CareerSettings.LayoffChance("expansion"), Is.EqualTo(0f));
            Assert.That(f.Tracks.Get("courier").LayoffSensitivity, Is.EqualTo(0f));
        }

        [Test]
        public void Income_IsFixedForSalaries_AndVariesForVolatileCareers()
        {
            var f = new SchoolWorkFixture(2030);
            var source = new CareerIncomeSource(f.Tracks, f.Economy, new Pcg32(4UL, 1UL));
            f.GiveJob("clerk");
            IncomeItem salary = source.MonthlyIncome(f.Character, f.Clock.Now).Single();
            f.Character.Career.End(JobEndReason.Quit, f.Clock.Now);
            f.GiveJob("courier", performance: 50f);
            List<decimal> months = Enumerable.Range(0, 24).Select(_ => source.MonthlyIncome(f.Character, f.Clock.Now).Sum(i => i.Amount)).ToList();

            Assert.That((salary.SourceKey, salary.Amount, salary.Taxable), Is.EqualTo(("career.clerk.name", 20000m, true)));
            Assert.That(months.Distinct().Count(), Is.GreaterThan(10));
            Assert.That(months, Is.All.GreaterThanOrEqualTo(0m));
            Assert.That(months.Average(), Is.EqualTo(30000m).Within(9000m));
        }
    }
}
