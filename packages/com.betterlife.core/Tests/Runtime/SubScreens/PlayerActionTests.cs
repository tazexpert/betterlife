using System.Linq;
using BetterLife.Core.Economy;
using BetterLife.Core.Education;
using BetterLife.Core.Localization;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using BetterLife.Core.Tests.Dashboard;
using BetterLife.Core.Tests.Saving;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.SubScreens
{
    /// <summary>Tests for the Life, Work and Invest view models and the player actions of task 17b.</summary>
    [TestFixture]
    public class PlayerActionTests
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
        public void LifePlan_EditsHours_WithoutCheckpoint_AndKeepsTheWeek()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 20);
                LifePlanState plan = LifePlanViewModel.Build(life, _keys, _money);
                ActivityRow free = plan.Activities.Single(a => a.Id == life.TimeState.Budget.DefaultActivityId);
                Assert.That(free.IsEditable, Is.False, "the default activity takes what is left");
                Assert.That(free.Note, Is.EqualTo(LifePlanViewModel.DefaultNoteKey));
                Assert.That(plan.Activities.Sum(a => a.Hours), Is.EqualTo(168));

                ActivityRow editable = plan.Activities.First(a => a.IsEditable && a.MaxHours > a.Hours);
                int target = editable.Stepped(5);
                Assert.That(_decisions.SetHours(life, editable.Id, target), Is.True);
                Assert.That(_undo.HasSnapshot, Is.False, "hours are not a checkpointed decision");

                LifePlanState after = LifePlanViewModel.Build(life, _keys, _money);
                Assert.That(after.Activities.Single(a => a.Id == editable.Id).Hours, Is.EqualTo(target));
                Assert.That(after.Activities.Sum(a => a.Hours), Is.EqualTo(168), "free time shrank by the same hours");
                Assert.That(editable.Stepped(-1000), Is.EqualTo(0));
                Assert.That(editable.Stepped(1000), Is.EqualTo(editable.MaxHours));
            }
        }

        [Test]
        public void LifePlan_ShowsWhatHoursDo_AndDoesNotTouchTheLifeRandomness()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 20);
                string before = SaveFixture.Snapshot(life);
                LifePlanState plan = LifePlanViewModel.Build(life, _keys, _money);

                Assert.That(plan.Activities.Where(a => a.Hours > 0).SelectMany(a => a.Effects), Is.Not.Empty);
                Assert.That(plan.Summary, Is.EqualTo(LifePlanViewModel.SummaryKey));
                Assert.That(SaveFixture.Snapshot(life), Is.EqualTo(before), "building the screen changes nothing that is saved");
            }
        }

        [Test]
        public void Work_ShowsSchoolAndJobs_AndApplying_TakesACheckpoint()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 25);
                if (life.Character.Career.IsEmployed)
                {
                    Assert.That(_decisions.QuitJob(life), Is.True);
                }
                WorkState work = WorkViewModel.Build(life, _keys, _money);
                Assert.That(work.CanQuitJob, Is.False);
                Assert.That(work.Job.Single().Label, Is.EqualTo(WorkViewModel.NotWorkingKey));
                Assert.That(work.JobOptions, Is.Not.Empty, "an adult can apply somewhere");

                JobOption job = work.JobOptions.First();
                ActionResultState result = WorkViewModel.BuildHiringResult(life, _decisions.ApplyJob(life, job.TrackId), _keys);
                Assert.That(_undo.HasSnapshot, Is.True);
                Assert.That(result.Title, Is.EqualTo(result.Succeeded ? WorkViewModel.HiredKey : WorkViewModel.NotHiredKey));
                Assert.That(WorkViewModel.Build(life, _keys, _money).CanQuitJob, Is.EqualTo(result.Succeeded));
            }
        }

        [Test]
        public void Work_SchoolOptions_PickWhoPays()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 6);
                WorkState child = WorkViewModel.Build(life, _keys, _money);
                Assert.That(child.EducationOptions.All(o => o.Funding == TuitionFunding.Family), Is.True, "children are paid for by family");
                Assert.That(child.JobOptions, Is.Empty);
            }
        }

        [Test]
        public void Invest_BuysSellsBorrowsAndRepays()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 22);
                life.Character.Wallet.Deposit(100000m);
                InvestState state = InvestViewModel.Build(life, _keys, _money);
                Assert.That(state.CanTrade, Is.True);
                Assert.That(state.Borrow.SelectMany(b => b.Choices).Any(c => c.IsAvailable), Is.False, "no income and no credit history: the bank says no");
                Assert.That(state.Borrow.Any(b => b.Id == "loan_shark"), Is.False, "the loan shark is only the automatic last resort");

                // A working adult with a credit history.
                life.Character.Finances.SetLastMonthlyIncome(30000m);
                life.Character.Stats.SetStored(BetterLife.Core.Domain.Stats.StatIds.CreditScore, 80f);

                OfferGroup fund = state.Buy.Single(b => b.Id == "index_fund");
                AmountChoice tenth = fund.Choices.First(c => c.IsAvailable);
                decimal cashBefore = life.Character.Wallet.Cash;
                _decisions.Buy(life, fund.Id, tenth.Amount);
                Assert.That(life.Character.Wallet.Cash, Is.EqualTo(cashBefore - tenth.Amount));
                Assert.That(_undo.HasSnapshot, Is.True);

                OfferGroup holding = InvestViewModel.Build(life, _keys, _money).Sell.Single();
                Assert.That(holding.Choices.Select(c => c.Amount), Is.EqualTo(new[] { 0.5m, 1m }));
                decimal received = _decisions.Sell(life, int.Parse(holding.Id), 1m);
                Assert.That(received, Is.GreaterThan(0m));
                Assert.That(InvestViewModel.Build(life, _keys, _money).Sell, Is.Empty);

                OfferGroup product = InvestViewModel.Build(life, _keys, _money).Borrow.FirstOrDefault(b => b.Choices.Any(c => c.IsAvailable && c.Amount > 0m));
                Assert.That(product, Is.Not.Null, "some unsecured loan is approved");
                AmountChoice amount = product.Choices.First(c => c.IsAvailable && c.Amount > 0m);
                Assert.That(amount.Detail, Is.EqualTo(InvestViewModel.LoanTermsKey), "approved loans show their terms");
                Debt debt = _decisions.TakeLoan(life, product.Id, amount.Amount);
                OfferGroup owed = InvestViewModel.Build(life, _keys, _money).Repay.Single(r => r.Id == debt.Id.ToString(System.Globalization.CultureInfo.InvariantCulture));
                AmountChoice payOff = owed.Choices.Last();
                Assert.That(_decisions.Repay(life, debt.Id, payOff.Amount), Is.EqualTo(payOff.Amount));
                Assert.That(life.Character.Finances.Debts.Find(debt.Id), Is.Null, "paid off and closed");
            }
        }

        [Test]
        public void Invest_ChildCannotTrade()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 10);
                life.Character.Wallet.Deposit(100000m);
                InvestState state = InvestViewModel.Build(life, _keys, _money);
                Assert.That(state.CanTrade, Is.False);
                Assert.That(state.Buy.SelectMany(b => b.Choices).Any(c => c.IsAvailable), Is.False);
                Assert.That(state.Borrow.SelectMany(b => b.Choices).Any(c => c.IsAvailable), Is.False);
            }
        }
    }
}
