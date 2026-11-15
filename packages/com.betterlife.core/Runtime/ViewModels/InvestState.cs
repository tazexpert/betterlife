using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// The invest and borrow screen.
    /// </summary>
    public sealed class InvestState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="canTrade">Whether the character is old enough to trade and borrow.</param>
        /// <param name="cashText">Cash available.</param>
        /// <param name="buy">Assets to buy, with amount buttons.</param>
        /// <param name="sell">Holdings to sell, with share buttons.</param>
        /// <param name="borrow">Loan products, with amount buttons (or one button for a credit line).</param>
        /// <param name="repay">Debts, with payment buttons.</param>
        public InvestState(bool canTrade, string cashText, IReadOnlyList<OfferGroup> buy, IReadOnlyList<OfferGroup> sell, IReadOnlyList<OfferGroup> borrow,
            IReadOnlyList<OfferGroup> repay)
        {
            CanTrade = canTrade;
            CashText = cashText;
            Buy = buy;
            Sell = sell;
            Borrow = borrow;
            Repay = repay;
        }

        /// <summary>Whether the character is old enough to trade and borrow.</summary>
        public bool CanTrade { get; }

        /// <summary>Cash available.</summary>
        public string CashText { get; }

        /// <summary>Assets to buy.</summary>
        public IReadOnlyList<OfferGroup> Buy { get; }

        /// <summary>Holdings to sell.</summary>
        public IReadOnlyList<OfferGroup> Sell { get; }

        /// <summary>Loan products.</summary>
        public IReadOnlyList<OfferGroup> Borrow { get; }

        /// <summary>Debts to repay.</summary>
        public IReadOnlyList<OfferGroup> Repay { get; }
    }
}
