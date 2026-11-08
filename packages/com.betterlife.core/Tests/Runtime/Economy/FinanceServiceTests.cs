using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Events;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Economy
{
    /// <summary>Tests for <see cref="FinanceService"/>.</summary>
    [TestFixture]
    public class FinanceServiceTests
    {
        private const decimal ModestCost = 12000m;

        [Test]
        public void Dependent_GetsFamilyAllowance_PaysNoLivingCosts_AndShortfallsAreCoveredByFamily()
        {
            var f = new FinanceFixture(2010, "wealthy");

            FinanceMonthReport report = f.Month();
            f.Finance.CoverShortfall(5000m, "test.source", f.Clock.Now);

            Assert.That(report.GrossIncome, Is.EqualTo(8000m));
            Assert.That(report.Total(FinanceLineCategory.Living), Is.EqualTo(0m));
            Assert.That(f.Character.Wallet.Cash, Is.EqualTo(8000m));
            Assert.That(f.Character.Finances.LifestyleTierId, Is.Null);
            Assert.That(f.Character.Finances.Debts.Debts, Is.Empty);
        }

        [Test]
        public void TurningEighteen_GivesTheFamilyGift_AndStartsLivingCosts()
        {
            var f = new FinanceFixture(2019, "wealthy");

            FinanceMonthReport report = f.Month();

            Assert.That(f.Character.Finances.LifestyleTierId, Is.EqualTo("modest"));
            Assert.That(report.Lines, Has.Some.Matches<FinanceLine>(l => l.SourceKey == FinanceService.GiftKey && l.Amount == 1000000m));
            Assert.That(report.Total(FinanceLineCategory.Living), Is.EqualTo(-ModestCost));
            Assert.That(f.Character.Wallet.Cash, Is.EqualTo(1000000m - ModestCost));
        }

        [Test]
        public void Inflation_ErodesCash()
        {
            var f = new FinanceFixture();
            f.Adult(100000m);

            FinanceMonthReport report = f.Month();

            Assert.That(report.Total(FinanceLineCategory.Inflation), Is.LessThan(0m));
            Assert.That(f.Character.Wallet.Cash, Is.EqualTo(100000m - ModestCost + report.Total(FinanceLineCategory.Inflation)));
        }

        [Test]
        public void BuyAndSell_MoveCash_AndChargeTheSellingFee()
        {
            var f = new FinanceFixture();
            f.Adult(10000m);

            Holding holding = f.Finance.Buy("index_fund", 5000m);
            decimal proceeds = f.Finance.Sell(holding.Id);

            Assert.That(proceeds, Is.EqualTo(4950m));
            Assert.That(f.Character.Wallet.Cash, Is.EqualTo(9950m));
            Assert.That(f.Character.Finances.Portfolio.Holdings, Is.Empty);
            Assert.Throws<ArgumentOutOfRangeException>(() => f.Finance.Buy("index_fund", 100m));
            Assert.Throws<InvalidOperationException>(() => f.Finance.Buy("index_fund", 50000m));
        }

        [Test]
        public void Dependents_CannotInvestOrBorrow()
        {
            var f = new FinanceFixture(2010);
            f.Character.Wallet.Deposit(10000m);

            Assert.Throws<InvalidOperationException>(() => f.Finance.Buy("index_fund", 1000m));
            Assert.That(f.Finance.Quote("personal_loan", 1000m).ReasonKeys, Does.Contain(LoanQuote.ReasonTooYoung));
        }

        [Test]
        public void Quote_ListsEveryReason()
        {
            var f = new FinanceFixture();
            f.Adult(0m);
            f.Character.ApplyStatChange(new StatChange(StatIds.CreditScore, -30f, "test.source"), f.Clock.Now);

            LoanQuote card = f.Finance.Quote("credit_card", 0m);
            LoanQuote personal = f.Finance.Quote("personal_loan", 0m);

            Assert.That(card.Approved, Is.False);
            Assert.That(card.ReasonKeys, Is.EquivalentTo(new[] { LoanQuote.ReasonCreditScore, LoanQuote.ReasonIncome }));
            Assert.That(personal.ReasonKeys, Is.EquivalentTo(new[] { LoanQuote.ReasonCreditScore, LoanQuote.ReasonAmount }));
        }

        [Test]
        public void TakeLoan_PaysOutCash_WithAnAnnuityInstallment()
        {
            var f = new FinanceFixture();
            f.Adult(0m);
            f.Character.Finances.SetLastMonthlyIncome(50000m);

            Debt loan = f.Finance.TakeLoan("personal_loan", 100000m);

            double r = (f.Economy.State.PolicyRate + 0.1f) / 12.0;
            decimal expected = Money.Round((decimal)(100000.0 * r / (1.0 - Math.Pow(1.0 + r, -60))));
            Assert.That(loan.MonthlyPayment, Is.EqualTo(expected));
            Assert.That(loan.MonthsRemaining, Is.EqualTo(60));
            Assert.That(f.Character.Wallet.Cash, Is.EqualTo(100000m));
            Assert.Throws<InvalidOperationException>(() => f.Finance.TakeLoan("personal_loan", 10000000m));
        }

        [Test]
        public void OnTimePayment_RaisesCreditScore()
        {
            var f = new FinanceFixture();
            f.Adult(500000m);
            f.Character.Finances.SetLastMonthlyIncome(50000m);
            f.Finance.TakeLoan("personal_loan", 100000m);

            f.Month();

            Assert.That(f.Character.Stats.GetValue(StatIds.CreditScore), Is.EqualTo(50f + f.Settings.Credit.OnTimeGain).Within(1e-4));
            Assert.That(f.Character.Finances.Debts.Debts.Single().MonthsRemaining, Is.EqualTo(59));
        }

        [Test]
        public void Shortfall_UsesCardRoomFirst_ThenTheLoanShark()
        {
            var f = new FinanceFixture();
            f.Adult(0m);
            f.Character.Finances.SetLastMonthlyIncome(20000m);
            Debt card = f.Finance.OpenCreditLine("credit_card");

            f.Bus.Publish(new CashShortfallEvent(40000m, "test.source", f.Clock.Now));

            Debt shark = f.Character.Finances.Debts.Debts.Single(d => d.Kind == DebtKind.Informal);
            Assert.That(card.CreditLimit, Is.EqualTo(30000m));
            Assert.That(card.Balance, Is.EqualTo(30000m));
            Assert.That(shark.Balance, Is.EqualTo(10000m));
            Assert.That(f.Character.Flags.IsSet(FinanceService.LoanSharkFlag), Is.True);
        }

        [Test]
        public void ThreeMissedPayments_DefaultTheMortgage_AndForecloseTheHome()
        {
            var f = new FinanceFixture();
            f.Adult(200000m);
            f.Character.Finances.SetLastMonthlyIncome(50000m);
            var foreclosures = new List<ForeclosureEvent>();
            var defaults = new List<DebtDefaultedEvent>();
            f.Bus.Subscribe<ForeclosureEvent>(foreclosures.Add);
            f.Bus.Subscribe<DebtDefaultedEvent>(defaults.Add);
            Debt mortgage = f.Finance.BuyWithLoan("condo_city", 1500000m, "mortgage");
            Assert.That(f.Character.Wallet.Cash, Is.EqualTo(50000m));
            Assert.Throws<InvalidOperationException>(() => f.Finance.Sell(mortgage.CollateralHoldingId.Value));
            f.EmptyWallet();

            f.Month();
            f.Month();
            Assert.That(foreclosures, Is.Empty);
            f.Month();

            Assert.That(defaults.Select(d => d.Debt.ProductId), Does.Contain("mortgage"));
            Assert.That(foreclosures.Single().AssetId, Is.EqualTo("condo_city"));
            Assert.That(f.Character.Finances.Portfolio.Holdings.Where(h => h.AssetId == "condo_city"), Is.Empty);
            Assert.That(f.LastTick.Interrupts, Is.Not.Empty);
            Assert.That(f.Character.Stats.GetValue(StatIds.CreditScore), Is.LessThan(20f));
        }

        [Test]
        public void DefaultingDeepInDebt_DeclaresBankruptcy_AndKeepsNonDischargeableDebt()
        {
            var f = new FinanceFixture();
            f.Adult(0m);
            f.Character.Finances.SetLastMonthlyIncome(100000m);
            var bankruptcies = new List<BankruptcyEvent>();
            f.Bus.Subscribe<BankruptcyEvent>(bankruptcies.Add);
            f.Finance.TakeLoan("personal_loan", 900000m);
            f.EmptyWallet();

            f.Month();
            f.Month();
            f.Month();

            Assert.That(bankruptcies, Has.Count.EqualTo(1));
            Assert.That(bankruptcies[0].DischargedDebt, Is.GreaterThan(800000m));
            Assert.That(f.Character.Finances.Debts.Debts.Select(d => d.ProductId), Is.EquivalentTo(new[] { "loan_shark" }));
            Assert.That(f.Character.Finances.IsBankrupt(f.Clock.Now), Is.True);
            Assert.That(f.Character.Flags.IsSet(FinanceService.BankruptFlag), Is.True);
            Assert.That(f.Character.Stats.GetValue(StatIds.CreditScore), Is.EqualTo(f.Settings.Credit.BankruptcyScore).Within(1e-3));
            Assert.That(f.Finance.Quote("personal_loan", 1000m).ReasonKeys, Does.Contain(LoanQuote.ReasonBankrupt));
        }

        [Test]
        public void Debt_AddsMonthlyStress_MoreForLoanSharks()
        {
            var f = new FinanceFixture();
            f.Adult(0m);
            f.Finance.CoverShortfall(ModestCost * 12m, "test.source", f.Clock.Now);

            f.Month();

            float stress = f.Character.ChangeLog.Entries.Where(e => e.SourceKey == FinanceService.DebtStressKey).Sum(e => e.Delta);
            Assert.That(stress, Is.GreaterThan(f.Settings.DebtStressPerYearOfCosts + f.Settings.InformalDebtStress - 0.01f));
            Assert.That(stress, Is.LessThanOrEqualTo(f.Settings.DebtStressMax + f.Settings.InformalDebtStress));
        }

        [Test]
        public void Wealth_FollowsTheCurveOfMonthsCovered()
        {
            var f = new FinanceFixture();
            f.Adult(ModestCost * 24m);

            FinanceMonthReport report = f.Month();

            float expected = f.Settings.WealthCurve.ScoreFor((float)(report.NetWorth / ModestCost));
            Assert.That(f.Character.Stats.GetValue(StatIds.Wealth), Is.EqualTo(expected).Within(0.01f));
            Assert.That(expected, Is.InRange(55f, 60f));
        }

        [Test]
        public void LifestyleCost_ScalesWithCostOfLiving()
        {
            var f = new FinanceFixture();
            f.Character.MoveTo(new BetterLife.Core.Domain.World.Residence("japan", null));

            Assert.That(f.Finance.LifestyleCost("modest"), Is.EqualTo(Money.Round(ModestCost * 1.8m)));
        }
    }
}
