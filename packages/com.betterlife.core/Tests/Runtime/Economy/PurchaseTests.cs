using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Sessions;
using BetterLife.Core.Tests.Dashboard;
using BetterLife.Core.Tests.Saving;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Economy
{
    /// <summary>Tests for task 26: buying a home or vehicle with cash or its loan.</summary>
    [TestFixture]
    public class PurchaseTests
    {
        private const string Condo = "home.condo";

        [Test]
        public void Children_CannotBuy()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                PurchaseQuote quote = life.Finance.QuotePurchase(Condo);
                Assert.That(quote.CashReasons, Does.Contain(LoanQuote.ReasonTooYoung));
                Assert.That(quote.LoanReasons, Does.Contain(LoanQuote.ReasonTooYoung));
            }
        }

        [Test]
        public void PayingCash_AddsTheHome_AndItsEffects()
        {
            using (LifeSession life = Adult())
            {
                life.Character.Wallet.Deposit(2000000m);
                life.Character.Stats.SetStored(StatIds.Happiness, 50f);
                decimal cash = life.Character.Wallet.Cash;
                float happiness = life.Character.Stats.GetValue(StatIds.Happiness);
                Assert.That(life.Finance.QuotePurchase(Condo).CashAllowed, Is.True);

                var effects = new DecisionService(null, 1).BuyPurchase(life, Condo, financed: false);

                Assert.That(life.Character.Wallet.Cash, Is.EqualTo(cash - 1500000m));
                Assert.That(life.Character.Finances.Portfolio.Holdings.Single().AssetId, Is.EqualTo("condo_city"));
                Assert.That(life.Character.Stats.GetValue(StatIds.Happiness), Is.GreaterThan(happiness));
                Assert.That(life.Finance.OwnsHome, Is.True);
                ActionResultState result = PurchaseViewModel.BuildResult(life, Condo, effects, DashboardFixture.Keys(), new MoneyFormatter(DashboardFixture.Keys()));
                Assert.That(result.Succeeded, Is.True);
                Assert.That(result.Lines, Is.Not.Empty);
            }
        }

        [Test]
        public void TheLoan_NeedsTheDownPaymentAndIncome_ThenFinancesTheRest()
        {
            using (LifeSession life = Adult())
            {
                if (life.Character.Wallet.Cash > 0m)
                {
                    life.Character.Wallet.WithdrawUpTo(life.Character.Wallet.Cash);
                }
                life.Character.Finances.SetLastMonthlyIncome(0m);
                PurchaseQuote refused = life.Finance.QuotePurchase(Condo);
                Assert.That(refused.LoanReasons, Does.Contain(LoanQuote.ReasonCash), "no money for the down payment");
                Assert.That(refused.LoanReasons, Does.Contain(LoanQuote.ReasonIncome));
                Assert.That(refused.CashReasons, Does.Contain(LoanQuote.ReasonCash));

                life.Character.Wallet.Deposit(200000m);
                life.Character.Finances.SetLastMonthlyIncome(100000m);
                life.Character.Stats.SetStored(StatIds.CreditScore, 80f);
                PurchaseQuote quote = life.Finance.QuotePurchase(Condo);
                Assert.That(quote.LoanAllowed, Is.True, string.Join(", ", quote.LoanReasons));
                Assert.That(quote.DownPayment, Is.EqualTo(150000m), "10% down");
                Assert.That(quote.TermMonths, Is.EqualTo(360));

                life.Finance.BuyPurchase(Condo, financed: true);
                Assert.That(life.Character.Wallet.Cash, Is.EqualTo(50000m));
                var debt = life.Character.Finances.Debts.Debts.Single(d => d.ProductId == "mortgage");
                Assert.That(debt.Balance, Is.EqualTo(1350000m));
                Assert.That(debt.CollateralHoldingId, Is.EqualTo(life.Character.Finances.Portfolio.Holdings.Single().Id));
            }
        }

        [Test]
        public void TheScreen_ListsHomesWithBothWaysToPay()
        {
            using (LifeSession life = Adult())
            {
                var offers = PurchaseViewModel.Build(life, PurchaseCategory.Home, DashboardFixture.Keys(), new MoneyFormatter(DashboardFixture.Keys()));
                Assert.That(offers.Single().Id, Is.EqualTo(Condo));
                Assert.That(offers.Single().Choices.Select(c => c.Id), Is.EqualTo(new[] { PurchaseViewModel.CashChoice, PurchaseViewModel.LoanChoice }));
                Assert.That(PurchaseViewModel.Build(life, PurchaseCategory.Vehicle, DashboardFixture.Keys(), new MoneyFormatter(DashboardFixture.Keys())), Is.Empty);
            }
        }

        private static LifeSession Adult()
        {
            LifeSession life = SaveFixture.NewLife();
            SaveFixture.Advance(life, 30);
            Assume.That(life.IsOver, Is.False);
            return life;
        }
    }
}
