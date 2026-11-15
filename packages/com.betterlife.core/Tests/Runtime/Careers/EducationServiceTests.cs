using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Education;
using BetterLife.Core.Events;
using BetterLife.Core.Life;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Careers
{
    /// <summary>Tests for <see cref="EducationService"/>.</summary>
    [TestFixture]
    public class EducationServiceTests
    {
        private const int WeeksPerYear = GameCalendar.WeeksPerYear;

        [Test]
        public void Dependents_AreEnrolledAtYearStart_OnceOldEnough_AndSchoolReservesHours()
        {
            var tooYoung = new SchoolWorkFixture(2006);
            tooYoung.Week();
            var f = new SchoolWorkFixture(2007);
            var enrolled = new List<EnrolledEvent>();
            f.Bus.Subscribe<EnrolledEvent>(enrolled.Add);

            f.Week();

            Assert.That(tooYoung.Character.Education.IsEnrolled, Is.False);
            Assert.That(f.Character.Education.Current.ProgramId, Is.EqualTo("primary"));
            Assert.That(f.Character.Education.Current.InstitutionId, Is.EqualTo("primary.public"));
            Assert.That(f.Character.Education.Current.Funding, Is.EqualTo(TuitionFunding.Family));
            Assert.That(f.Budget.GetHours("school"), Is.EqualTo(30));
            Assert.That(enrolled, Has.Count.EqualTo(1));
        }

        [Test]
        public void WealthyFamilies_PickTheirSchool_AndPayTuition()
        {
            var f = new SchoolWorkFixture(2007, "wealthy");

            f.Week();

            Assert.That(f.Character.Education.Current.InstitutionId, Is.EqualTo("primary.private"));
            Assert.That(f.Character.Wallet.Cash, Is.EqualTo(0m));
        }

        [Test]
        public void Performance_FollowsAttendance()
        {
            var diligent = new SchoolWorkFixture(2007);
            var absent = new SchoolWorkFixture(2007);
            for (int i = 0; i < 20; i++)
            {
                diligent.Week(school: 30, study: 10);
                absent.Week();
            }

            float good = diligent.Character.Education.Current.Performance;
            float bad = absent.Character.Education.Current.Performance;
            Assert.That(good, Is.GreaterThan(diligent.EducationSettings.StartPerformance));
            Assert.That(bad, Is.LessThan(absent.EducationSettings.PassPerformance));
            Assert.That(diligent.Character.Education.Current.Gpa, Is.EqualTo(good / 25f).Within(1e-4));
            Assert.That(diligent.Character.Skills.GetLevel("cooking"), Is.GreaterThan(0f));
        }

        [Test]
        public void PassingYears_Advances_FailingRepeats_AndTooManyRepeatsExpel()
        {
            var pass = new SchoolWorkFixture(2007);
            var fail = new SchoolWorkFixture(2007);
            var years = new List<SchoolYearEndedEvent>();
            var ended = new List<EducationEndedEvent>();
            fail.Bus.Subscribe<SchoolYearEndedEvent>(years.Add);
            fail.Bus.Subscribe<EducationEndedEvent>(ended.Add);

            for (int i = 0; i <= WeeksPerYear; i++)
            {
                pass.Week(school: 30);
            }
            for (int i = 0; i <= WeeksPerYear * 3; i++)
            {
                fail.Week();
            }

            Assert.That(pass.Character.Education.Current.YearsCompleted, Is.EqualTo(1));
            Assert.That(years.Take(3).Select(y => y.Passed), Is.All.False);
            Assert.That(ended.Single().Reason, Is.EqualTo(EducationEndReason.Expelled));
            Assert.That(fail.Budget.GetHours("school"), Is.EqualTo(0));
        }

        [Test]
        public void Graduating_AddsACredential_AndDependentsMoveOnToTheNextLevel()
        {
            var f = new SchoolWorkFixture(2012);
            f.Character.Education.Restore(new Enrollment("primary", "primary.public", GameDate.FromYearWeek(2006, 0), GameDate.FromYearWeek(2011, 0),
                5, 0, 80f, false, TuitionFunding.Family), null, null);

            f.Week(school: 30);

            Credential diploma = f.Character.Education.Credentials.Single();
            Assert.That((diploma.ProgramId, diploma.GraduatedOn), Is.EqualTo(("primary", GameDate.FromYearWeek(2012, 0))));
            Assert.That(f.Character.Education.HighestRank(f.Programs), Is.EqualTo(2));
            Assert.That(f.Character.Education.Current.ProgramId, Is.EqualTo("secondary"));
            Assert.That(f.LastTick.Interrupts.Select(i => i.ReasonKey), Does.Contain(EducationService.GraduatedInterruptKey));
        }

        [Test]
        public void CompulsorySchool_CannotBeLeftByDependents()
        {
            var f = new SchoolWorkFixture(2007);
            f.Week();

            Assert.That(f.Education.DropOut(), Is.False);
            f.Dependency.Independent = true;
            Assert.That(f.Education.DropOut(), Is.True);
            Assert.That(f.Character.Education.IsEnrolled, Is.False);
        }

        [Test]
        public void Quote_ListsEveryReason()
        {
            var child = new SchoolWorkFixture(2010);
            var adult = new SchoolWorkFixture(2020);
            adult.Graduate("primary", "primary.public", 70f);
            adult.Graduate("secondary", "secondary.public", 70f);
            adult.Graduate("bachelor", "bachelor.open", 70f);

            Assert.That(child.Education.Quote("bachelor", "bachelor.open").ReasonKeys,
                Is.EquivalentTo(new[] { AdmissionQuote.ReasonTooYoung, AdmissionQuote.ReasonRequirement }));
            Assert.That(adult.Education.Quote("bachelor", "bachelor.open").ReasonKeys, Is.EquivalentTo(new[] { AdmissionQuote.ReasonCompleted }));
            Assert.That(adult.Education.Quote("primary", "primary.public").ReasonKeys, Does.Contain(AdmissionQuote.ReasonTooOld));
        }

        [Test]
        public void ExamOdds_FollowTheScore_AndAFailedExamMustWaitAYear()
        {
            var strong = new SchoolWorkFixture(2019);
            var weak = new SchoolWorkFixture(2019);
            strong.Graduate("secondary", "secondary.public", 95f);
            weak.Graduate("secondary", "secondary.public", 45f);
            strong.Character.ApplyStatChange(new StatChange(StatIds.Knowledge, 90f, "test.source"), strong.Clock.Now);

            AdmissionQuote high = strong.Education.Quote("bachelor", "bachelor.top");
            AdmissionQuote low = weak.Education.Quote("bachelor", "bachelor.top");

            Assert.That(high.ExamScore, Is.GreaterThan(low.ExamScore));
            Assert.That(high.Chance.Final, Is.GreaterThan(low.Chance.Final));
            Assert.That(high.ScholarshipPossible, Is.True);
            Assert.That(low.ScholarshipPossible, Is.False);
            AdmissionResult result = weak.Education.Apply("bachelor", "bachelor.top");
            if (!result.Admitted)
            {
                Assert.That(weak.Education.Quote("bachelor", "bachelor.top").ReasonKeys, Does.Contain(AdmissionQuote.ReasonRetryWait));
                Assert.That(weak.Education.Quote("bachelor", "bachelor.open").ReasonKeys, Is.Empty);
            }
        }

        [Test]
        public void OpenAdmission_AlwaysAdmits_AndAdultStudentsPayTuition_WithAStudentLoanWhenCashIsShort()
        {
            var cash = new SchoolWorkFixture(2019);
            var broke = new SchoolWorkFixture(2019);
            var loan = new SchoolWorkFixture(2019);
            var shortCash = new SchoolWorkFixture(2019);
            foreach (SchoolWorkFixture f in new[] { cash, broke, loan, shortCash })
            {
                f.Dependency.Independent = true;
                f.Graduate("secondary", "secondary.public", 50f);
            }
            cash.Character.Wallet.Deposit(25000m);
            shortCash.Character.Wallet.Deposit(4000m);
            broke.Loans.Approve = false;
            var shortfalls = new List<CashShortfallEvent>();
            broke.Bus.Subscribe<CashShortfallEvent>(shortfalls.Add);

            AdmissionResult paid = cash.Education.Apply("bachelor", "bachelor.open", TuitionFunding.Cash);
            broke.Education.Apply("bachelor", "bachelor.open", TuitionFunding.Cash);
            loan.Education.Apply("bachelor", "bachelor.open", TuitionFunding.StudentLoan);
            shortCash.Education.Apply("bachelor", "bachelor.open", TuitionFunding.Cash);

            Assert.That(paid.Admitted, Is.True);
            Assert.That(paid.Roll, Is.Null);
            Assert.That(cash.Character.Wallet.Cash, Is.EqualTo(15000m));
            Assert.That(shortfalls.Single().Amount, Is.EqualTo(10000m));
            Assert.That(loan.Loans.Taken, Is.EqualTo(new[] { 10000m }));
            Assert.That(loan.Character.Wallet.Cash, Is.EqualTo(0m));
            Assert.That(shortCash.Loans.Taken, Is.EqualTo(new[] { 6000m }));
            Assert.That(shortCash.Character.Wallet.Cash, Is.EqualTo(0m));
        }

        [Test]
        public void Scholarships_WaiveTuition_UntilPerformanceDrops()
        {
            var keep = new SchoolWorkFixture(2021);
            var lose = new SchoolWorkFixture(2021);
            foreach (SchoolWorkFixture f in new[] { keep, lose })
            {
                f.Dependency.Independent = true;
                f.Character.Wallet.Deposit(100000m);
            }
            keep.Character.Education.Restore(new Enrollment("bachelor", "bachelor.top", GameDate.FromYearWeek(2020, 0), GameDate.FromYearWeek(2020, 0),
                0, 0, 95f, true, TuitionFunding.Cash), null, null);
            lose.Character.Education.Restore(new Enrollment("bachelor", "bachelor.top", GameDate.FromYearWeek(2020, 0), GameDate.FromYearWeek(2020, 0),
                0, 0, 60f, true, TuitionFunding.Cash), null, null);

            keep.Week(school: 25, study: 20);
            lose.Week(school: 25);

            Assert.That(keep.Character.Education.Current.Scholarship, Is.True);
            Assert.That(keep.Character.Wallet.Cash, Is.EqualTo(100000m));
            Assert.That(lose.Character.Education.Current.Scholarship, Is.False);
            Assert.That(lose.Character.Wallet.Cash, Is.EqualTo(70000m));
        }

        [Test]
        public void StageChange_ReservesSchoolHoursAgain()
        {
            var f = new SchoolWorkFixture(2007);
            f.Week();
            f.Budget.Reset(new Dictionary<string, int>());

            var stages = TestCatalogs.LifeStages();
            f.Bus.Publish(new LifeStageChangedEvent(f.Character, stages.All[1], stages.All[2], f.Clock.Now));

            Assert.That(f.Budget.GetHours("school"), Is.EqualTo(30));
        }
    }
}
