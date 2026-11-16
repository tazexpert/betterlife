using System.Collections.Generic;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// What a big purchase would cost now: paying cash, or the down payment and installments of its loan, and why
    /// either way is not possible.
    /// </summary>
    public sealed class PurchaseQuote
    {
        /// <summary>
        /// Creates the quote.
        /// </summary>
        /// <param name="price">Price (real).</param>
        /// <param name="cashReasons">Reasons paying cash is not possible (empty = possible).</param>
        /// <param name="downPayment">Down payment of the loan (real).</param>
        /// <param name="loan">Loan terms for the rest of the price.</param>
        /// <param name="loanReasons">Reasons the loan is not possible (empty = possible).</param>
        /// <param name="termMonths">Loan length in months.</param>
        public PurchaseQuote(decimal price, IReadOnlyList<string> cashReasons, decimal downPayment, LoanQuote loan, IReadOnlyList<string> loanReasons, int termMonths)
        {
            Price = price;
            CashReasons = cashReasons;
            DownPayment = downPayment;
            Loan = loan;
            LoanReasons = loanReasons;
            TermMonths = termMonths;
        }

        /// <summary>Price (real).</summary>
        public decimal Price { get; }

        /// <summary>Reasons paying cash is not possible (localization keys; empty = possible).</summary>
        public IReadOnlyList<string> CashReasons { get; }

        /// <summary>True when the character can pay cash.</summary>
        public bool CashAllowed => CashReasons.Count == 0;

        /// <summary>Down payment of the loan (real).</summary>
        public decimal DownPayment { get; }

        /// <summary>Loan terms for the rest of the price.</summary>
        public LoanQuote Loan { get; }

        /// <summary>Reasons the loan is not possible (localization keys; empty = possible).</summary>
        public IReadOnlyList<string> LoanReasons { get; }

        /// <summary>True when the bank lends and the down payment is affordable.</summary>
        public bool LoanAllowed => LoanReasons.Count == 0;

        /// <summary>Loan length in months.</summary>
        public int TermMonths { get; }
    }
}
