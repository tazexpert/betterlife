using System;
using System.Collections.Generic;
using System.Globalization;
using BetterLife.Core.Economy;
using BetterLife.Core.Sessions;
using BetterLife.Core.ViewModels;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Invest and borrow (pushed from the Money tab): buy, sell, borrow and repay.
    /// </summary>
    public sealed class InvestScreenPresenter : ListScreenPresenter
    {
        /// <summary>
        /// Creates the screen.
        /// </summary>
        /// <param name="context">Screen context.</param>
        public InvestScreenPresenter(ScreenContext context)
            : base(context, "invest.title")
        {
        }

        /// <inheritdoc />
        protected override void Fill(LifeSession session)
        {
            InvestState state = InvestViewModel.Build(session, Context.Localizer, Context.Money);
            Content.Add(UiRows.Row(Context.Localizer.Get(FinanceViewModel.CashKey), state.CashText, string.Empty, RowTone.Neutral));
            if (!state.CanTrade)
            {
                Content.Add(UiRows.Text(Context.Localizer.Get("invest.too_young"), "list-screen__empty"));
                return;
            }
            bool running = !session.IsOver;
            AddOffers("invest.section.repay", state.Repay, running, (offer, choice) => Repay(offer, choice));
            AddOffers("invest.section.sell", state.Sell, running, (offer, choice) => Sell(offer, choice));
            AddOffers("invest.section.buy", state.Buy, running, (offer, choice) => Buy(offer, choice));
            AddOffers("invest.section.borrow", state.Borrow, running, (offer, choice) => Borrow(offer, choice));
        }

        private void AddOffers(string titleKey, IReadOnlyList<OfferGroup> offers, bool running, Action<OfferGroup, AmountChoice> act)
        {
            if (offers.Count == 0)
            {
                return;
            }
            Content.Add(UiRows.Section(Context.Localizer.Get(titleKey)));
            foreach (OfferGroup offer in offers)
            {
                Content.Add(UiRows.Row(offer.Title, string.Empty, offer.Detail, RowTone.Neutral));
                foreach (AmountChoice choice in offer.Choices)
                {
                    OfferGroup o = offer;
                    AmountChoice c = choice;
                    Content.Add(UiRows.Action(choice.Label, () => act(o, c), choice.IsAvailable && running, choice.Detail));
                }
            }
        }

        private void Buy(OfferGroup asset, AmountChoice choice)
        {
            LifeSession session = Context.Session;
            if (session == null)
            {
                return;
            }
            Context.Decisions.Buy(session, asset.Id, choice.Amount);
            ShowResult(InvestViewModel.BuildResult(session, InvestViewModel.BoughtKey, choice.Amount, Context.Localizer, Context.Money));
        }

        private void Sell(OfferGroup holding, AmountChoice choice)
        {
            LifeSession session = Context.Session;
            if (session == null)
            {
                return;
            }
            decimal received = Context.Decisions.Sell(session, int.Parse(holding.Id, CultureInfo.InvariantCulture), choice.Amount);
            ShowResult(InvestViewModel.BuildResult(session, InvestViewModel.SoldKey, received, Context.Localizer, Context.Money));
        }

        private void Borrow(OfferGroup product, AmountChoice choice)
        {
            LifeSession session = Context.Session;
            if (session == null)
            {
                return;
            }
            if (choice.Amount > 0m)
            {
                Context.Decisions.TakeLoan(session, product.Id, choice.Amount);
                ShowResult(InvestViewModel.BuildResult(session, InvestViewModel.BorrowedKey, choice.Amount, Context.Localizer, Context.Money));
                return;
            }
            Debt line = Context.Decisions.OpenCreditLine(session, product.Id);
            ShowResult(InvestViewModel.BuildResult(session, InvestViewModel.OpenedKey, line.CreditLimit, Context.Localizer, Context.Money));
        }

        private void Repay(OfferGroup debt, AmountChoice choice)
        {
            LifeSession session = Context.Session;
            if (session == null)
            {
                return;
            }
            decimal paid = Context.Decisions.Repay(session, int.Parse(debt.Id, CultureInfo.InvariantCulture), choice.Amount);
            ShowResult(InvestViewModel.BuildResult(session, InvestViewModel.RepaidKey, paid, Context.Localizer, Context.Money));
        }
    }
}
