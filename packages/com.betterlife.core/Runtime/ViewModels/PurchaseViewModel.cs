using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Economy;
using BetterLife.Core.Localization;
using BetterLife.Core.Rules;
using BetterLife.Core.Sessions;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// The homes-and-vehicles screen: every big purchase with its price, how many the player owns, and two ways to buy
    /// (cash, or the bank loan with its down payment and installment), each with the reasons it is not possible.
    /// </summary>
    public static class PurchaseViewModel
    {
        /// <summary>Screen title.</summary>
        public const string TitleKey = "purchase.title";
        /// <summary>Button on the money screen.</summary>
        public const string OpenKey = "purchase.open";
        /// <summary>Section title of homes.</summary>
        public const string HomesKey = "purchase.section.home";
        /// <summary>Section title of vehicles.</summary>
        public const string VehiclesKey = "purchase.section.vehicle";
        /// <summary>Item detail (placeholders <c>{price}</c>, <c>{owned}</c>).</summary>
        public const string DetailKey = "purchase.detail";
        /// <summary>Cash choice (placeholder <c>{price}</c>).</summary>
        public const string CashKey = "purchase.cash";
        /// <summary>Loan choice (placeholders <c>{down}</c>, <c>{payment}</c>).</summary>
        public const string LoanKey = "purchase.loan";
        /// <summary>Loan terms (placeholders <c>{rate}</c>, <c>{years}</c>).</summary>
        public const string LoanTermsKey = "purchase.loan_terms";
        /// <summary>Result title (placeholder <c>{name}</c>).</summary>
        public const string BoughtKey = "purchase.bought";
        /// <summary>Hint about the home owner's saving (placeholder <c>{value}</c>).</summary>
        public const string HomeDiscountKey = "purchase.home_discount";
        /// <summary>Shown to dependents.</summary>
        public const string AdultsOnlyKey = "purchase.adults_only";

        /// <summary>Id of the cash choice.</summary>
        public const string CashChoice = "cash";
        /// <summary>Id of the loan choice.</summary>
        public const string LoanChoice = "loan";

        private const decimal Hundred = 100m;
        private const string Separator = " · ";

        /// <summary>
        /// Builds the offers of one category.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="category">Homes or vehicles.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>One group per purchase, in price order.</returns>
        public static IReadOnlyList<OfferGroup> Build(LifeSession session, PurchaseCategory category, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            if (localizer == null || money == null)
            {
                throw new ArgumentNullException(nameof(localizer));
            }
            FinanceService finance = session.Finance;
            string Money(decimal real) => SessionText.Money(session, money, real);
            return finance.Purchases.All.Where(p => p.Category == category).OrderBy(p => p.Price).Select(p =>
            {
                PurchaseQuote quote = finance.QuotePurchase(p.Id);
                int owned = session.Character.Finances.Portfolio.Holdings.Count(h => h.AssetId == p.AssetId);
                string detail = localizer.Get(p.DescKey) + Separator
                    + localizer.Format(DetailKey, ("price", Money(p.Price)), ("owned", owned.ToString(CultureInfo.InvariantCulture)));
                var choices = new[]
                {
                    new AmountChoice(CashChoice, localizer.Format(CashKey, ("price", Money(p.Price))), p.Price, quote.CashAllowed, Reasons(quote.CashReasons, localizer)),
                    new AmountChoice(LoanChoice, localizer.Format(LoanKey, ("down", Money(quote.DownPayment)), ("payment", Money(quote.Loan.MonthlyPayment))), p.Price,
                        quote.LoanAllowed, quote.LoanAllowed
                            ? localizer.Format(LoanTermsKey, ("rate", SessionText.Percent(quote.Loan.AnnualRate * (float)Hundred, localizer)),
                                ("years", (quote.TermMonths / Economy.Money.MonthsPerYear).ToString(CultureInfo.InvariantCulture)))
                            : Reasons(quote.LoanReasons, localizer)),
                };
                return new OfferGroup(p.Id, localizer.Get(p.NameKey), detail, choices);
            }).ToList();
        }

        /// <summary>
        /// The hint shown above the homes: owning a home lowers the monthly living costs.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The hint.</returns>
        public static string HomeDiscountHint(LifeSession session, ILocalizer localizer)
        {
            if (session == null || localizer == null)
            {
                throw new ArgumentNullException(session == null ? nameof(session) : nameof(localizer));
            }
            float discount = session.Finance.HomeOwnerLivingDiscount * (float)Hundred;
            return localizer.Format(HomeDiscountKey, ("value", SessionText.Percent(discount, localizer)));
        }

        /// <summary>
        /// The result card after buying.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="purchaseId">Purchase id.</param>
        /// <param name="effects">Effects of owning it.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state.</returns>
        public static ActionResultState BuildResult(LifeSession session, string purchaseId, EffectResult effects, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null || effects == null || localizer == null)
            {
                throw new ArgumentNullException(session == null ? nameof(session) : effects == null ? nameof(effects) : nameof(localizer));
            }
            PurchaseDefinition purchase = session.Finance.Purchases.Get(purchaseId);
            IReadOnlyList<string> lines = new RuleTextFormatter(localizer, money ?? new MoneyFormatter(localizer), session.Economy.State.Nominal).FormatAll(effects.Lines);
            return new ActionResultState(localizer.Format(BoughtKey, ("name", localizer.Get(purchase.NameKey))), true, lines);
        }

        private static string Reasons(IReadOnlyList<string> keys, ILocalizer localizer) => string.Join(Separator, keys.Select(localizer.Get));
    }
}
