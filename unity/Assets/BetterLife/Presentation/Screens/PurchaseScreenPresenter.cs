using System.Collections.Generic;
using BetterLife.Core.Economy;
using BetterLife.Core.Rules;
using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Homes and vehicles: each with its price and two ways to buy (cash, or the bank loan with down payment and installment).
    /// A choice that is not possible shows why.
    /// </summary>
    public sealed class PurchaseScreenPresenter : ListScreenPresenter
    {
        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public PurchaseScreenPresenter(ScreenContext context)
            : base(context, PurchaseViewModel.TitleKey)
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            Content.Add(UiRows.Row(Context.Localizer.Get(FinanceViewModel.CashKey), SessionText.Money(session, Context.Money, session.Character.Wallet.Cash),
                string.Empty, RowTone.Neutral));
            if (!session.Finance.IsAdult(session.TimeState.Clock.Now))
            {
                Content.Add(UiRows.Text(Context.Localizer.Get(PurchaseViewModel.AdultsOnlyKey), "list-screen__empty"));
            }
            bool running = !session.IsOver;
            Content.Add(UiRows.Section(Context.Localizer.Get(PurchaseViewModel.HomesKey)));
            Content.Add(UiRows.Text(PurchaseViewModel.HomeDiscountHint(session, Context.Localizer), "list-screen__subtitle"));
            AddOffers(PurchaseViewModel.Build(session, PurchaseCategory.Home, Context.Localizer, Context.Money), running);
            Content.Add(UiRows.Section(Context.Localizer.Get(PurchaseViewModel.VehiclesKey)));
            AddOffers(PurchaseViewModel.Build(session, PurchaseCategory.Vehicle, Context.Localizer, Context.Money), running);
        }

        private void AddOffers(IReadOnlyList<OfferGroup> offers, bool running)
        {
            foreach (OfferGroup offer in offers)
            {
                Content.Add(UiRows.Row(offer.Title, string.Empty, offer.Detail, RowTone.Neutral));
                foreach (AmountChoice choice in offer.Choices)
                {
                    string purchaseId = offer.Id;
                    bool financed = choice.Id == PurchaseViewModel.LoanChoice;
                    Content.Add(UiRows.Action(choice.Label, () => Buy(purchaseId, financed), choice.IsAvailable && running, choice.Detail));
                }
            }
        }

        private void Buy(string purchaseId, bool financed)
        {
            LifeSession session = Context.Session;
            if (session == null)
            {
                return;
            }
            EffectResult effects = Context.Decisions.BuyPurchase(session, purchaseId, financed);
            ShowResult(PurchaseViewModel.BuildResult(session, purchaseId, effects, Context.Localizer, Context.Money));
        }
    }
}
