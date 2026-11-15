using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Economy;
using BetterLife.Core.Localization;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the invest and borrow screen: buy with a share of cash, sell half or all, take unsecured loans or open
    /// a credit line, and repay debts. Secured loans (homes, cars) are not offered here yet; student loans come with school
    /// and the loan shark is only the automatic last resort when cash runs out.
    /// </summary>
    public static class InvestViewModel
    {
        /// <summary>Shares of cash offered when buying.</summary>
        public static readonly IReadOnlyList<decimal> BuyShares = new[] { 0.1m, 0.25m, 0.5m, 1m };
        /// <summary>Real loan amounts offered.</summary>
        public static readonly IReadOnlyList<decimal> LoanAmounts = new[] { 10000m, 50000m, 200000m };
        /// <summary>Shares offered when selling.</summary>
        public static readonly IReadOnlyList<decimal> SellShares = new[] { 0.5m, 1m };

        /// <summary>Buy button (placeholders <c>{share}</c>, <c>{value}</c>).</summary>
        public const string BuyKey = "invest.buy";
        /// <summary>Asset detail (placeholders <c>{class}</c>, <c>{min}</c>, <c>{fee}</c>).</summary>
        public const string AssetDetailKey = "invest.asset.detail";
        /// <summary>Asset class key prefix (<c>invest.class.stock</c>...).</summary>
        public const string ClassPrefix = "invest.class.";
        /// <summary>Below the minimum purchase.</summary>
        public const string BelowMinimumKey = "invest.below_minimum";
        /// <summary>Sell half button (placeholder <c>{value}</c>).</summary>
        public const string SellHalfKey = "invest.sell_half";
        /// <summary>Sell all button (placeholder <c>{value}</c>).</summary>
        public const string SellAllKey = "invest.sell_all";
        /// <summary>Holding that secures a loan.</summary>
        public const string CollateralKey = "invest.collateral";
        /// <summary>Borrow button (placeholder <c>{value}</c>).</summary>
        public const string BorrowKey = "invest.borrow";
        /// <summary>Loan terms (placeholders <c>{payment}</c>, <c>{rate}</c>).</summary>
        public const string LoanTermsKey = "invest.loan.terms";
        /// <summary>Open credit line button (placeholder <c>{value}</c> = limit).</summary>
        public const string CreditLineKey = "invest.credit_line";
        /// <summary>Already has this credit line.</summary>
        public const string HasCreditLineKey = "invest.has_credit_line";
        /// <summary>Pay this month button (placeholder <c>{value}</c>).</summary>
        public const string PayMonthKey = "invest.pay_month";
        /// <summary>Pay off button (placeholder <c>{value}</c>).</summary>
        public const string PayOffKey = "invest.pay_off";
        /// <summary>Not enough cash.</summary>
        public const string NoCashKey = "invest.no_cash";
        /// <summary>Result keys.</summary>
        public const string BoughtKey = "invest.result.bought";
        /// <summary>Sold result (placeholder <c>{value}</c> = cash received).</summary>
        public const string SoldKey = "invest.result.sold";
        /// <summary>Borrowed result (placeholder <c>{value}</c>).</summary>
        public const string BorrowedKey = "invest.result.borrowed";
        /// <summary>Credit line opened result (placeholder <c>{value}</c> = limit).</summary>
        public const string OpenedKey = "invest.result.opened";
        /// <summary>Repaid result (placeholder <c>{value}</c>).</summary>
        public const string RepaidKey = "invest.result.repaid";

        /// <summary>Credit line terms (placeholder <c>{rate}</c>).</summary>
        public const string RevolvingTermsKey = "invest.loan.revolving";
        /// <summary>Format of the cash share on buy buttons.</summary>
        public const string ShareFormat = "0";

        private const decimal Hundred = 100m;

        /// <summary>
        /// Builds the screen.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state.</returns>
        public static InvestState Build(LifeSession session, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (localizer == null || money == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            GameDate now = session.TimeState.Clock.Now;
            FinanceService finance = session.Finance;
            decimal cash = session.Character.Wallet.Cash;
            string Money(decimal real) => SessionText.Money(session, money, real);
            bool adult = finance.IsAdult(now);

            List<OfferGroup> buy = finance.Assets.All.Select(asset =>
            {
                string detail = localizer.Format(AssetDetailKey, ("class", localizer.Get(SessionText.EnumKey(ClassPrefix, asset.Class))),
                    ("min", Money(asset.MinPurchase)), ("fee", SessionText.Percent((float)(asset.SellFeeRate * Hundred), localizer)));
                List<AmountChoice> choices = BuyShares.Select(share =>
                {
                    decimal amount = Economy.Money.Round(cash * share);
                    bool ok = adult && amount >= asset.MinPurchase && amount > 0m;
                    return new AmountChoice(null, localizer.Format(BuyKey, ("share", ((float)(share * Hundred)).ToString(ShareFormat, CultureInfo.InvariantCulture)), ("value", Money(amount))),
                        amount, ok, ok ? string.Empty : localizer.Get(BelowMinimumKey));
                }).ToList();
                return new OfferGroup(asset.Id, localizer.Get(asset.NameKey), detail, choices);
            }).ToList();

            List<OfferGroup> sell = session.Character.Finances.Portfolio.Holdings.Select(holding =>
            {
                bool pledged = session.Character.Finances.Debts.Debts.Any(d => d.CollateralHoldingId == holding.Id);
                string name = finance.Assets.TryGet(holding.AssetId, out AssetDefinition asset) ? localizer.Get(asset.NameKey) : holding.AssetId;
                List<AmountChoice> choices = SellShares.Select(share => new AmountChoice(null,
                    localizer.Format(share < 1m ? SellHalfKey : SellAllKey, ("value", Money(holding.Value * share))), share, !pledged)).ToList();
                return new OfferGroup(holding.Id.ToString(CultureInfo.InvariantCulture), name, pledged ? localizer.Get(CollateralKey) : Money(holding.Value), choices);
            }).ToList();

            var borrow = new List<OfferGroup>();
            foreach (DebtProductDefinition product in finance.Products.All.Where(p => !p.CollateralClass.HasValue && p.Kind != DebtKind.Student && p.Kind != DebtKind.Informal))
            {
                string name = localizer.Get(product.NameKey);
                if (product.IsRevolving)
                {
                    bool has = session.Character.Finances.Debts.Debts.Any(d => d.ProductId == product.Id);
                    LoanQuote quote = finance.Quote(product.Id, 0m);
                    string reason = has ? localizer.Get(HasCreditLineKey) : Reasons(quote, localizer);
                    borrow.Add(new OfferGroup(product.Id, name, localizer.Format(RevolvingTermsKey, ("rate", Rate(quote, localizer))), new[]
                    {
                        new AmountChoice(null, localizer.Format(CreditLineKey, ("value", Money(quote.CreditLimit))), 0m, !has && quote.Approved, reason),
                    }));
                    continue;
                }
                List<AmountChoice> amounts = LoanAmounts.Select(amount =>
                {
                    LoanQuote quote = finance.Quote(product.Id, amount);
                    string detail = quote.Approved
                        ? localizer.Format(LoanTermsKey, ("payment", Money(quote.MonthlyPayment)), ("rate", Rate(quote, localizer)))
                        : Reasons(quote, localizer);
                    return new AmountChoice(null, localizer.Format(BorrowKey, ("value", Money(amount))), amount, quote.Approved, detail);
                }).ToList();
                borrow.Add(new OfferGroup(product.Id, name, string.Empty, amounts));
            }

            List<OfferGroup> repay = session.Character.Finances.Debts.Debts.Where(d => d.Balance > 0m).Select(debt =>
            {
                string name = finance.Products.TryGet(debt.ProductId, out DebtProductDefinition product) ? localizer.Get(product.NameKey) : debt.ProductId;
                var choices = new List<AmountChoice>();
                decimal month = Math.Min(debt.MonthlyPayment, debt.Balance);
                if (month > 0m && month < debt.Balance)
                {
                    choices.Add(new AmountChoice(null, localizer.Format(PayMonthKey, ("value", Money(month))), month, cash >= month,
                        cash >= month ? string.Empty : localizer.Get(NoCashKey)));
                }
                decimal all = Math.Min(debt.Balance, cash);
                choices.Add(new AmountChoice(null, localizer.Format(PayOffKey, ("value", Money(all))), all, all > 0m,
                    all > 0m ? string.Empty : localizer.Get(NoCashKey)));
                return new OfferGroup(debt.Id.ToString(CultureInfo.InvariantCulture), name, Money(debt.Balance), choices);
            }).ToList();

            return new InvestState(adult, Money(cash), buy, sell, borrow, repay);
        }

        /// <summary>
        /// Builds a one-line result of a money action.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="key">Result key (<see cref="BoughtKey"/>...).</param>
        /// <param name="real">Real amount shown as <c>{value}</c>.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state.</returns>
        public static ActionResultState BuildResult(LifeSession session, string key, decimal real, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            return new ActionResultState(localizer.Format(key, ("value", SessionText.Money(session, money, real))), true, new string[0]);
        }

        private static string Rate(LoanQuote quote, ILocalizer localizer) => SessionText.Percent(quote.AnnualRate * (float)Hundred, localizer);

        private static string Reasons(LoanQuote quote, ILocalizer localizer) => string.Join(" · ", quote.ReasonKeys.Select(localizer.Get));
    }
}
