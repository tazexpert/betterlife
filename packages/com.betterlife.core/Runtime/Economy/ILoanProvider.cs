namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Lends money to the character (used by systems that let the player borrow, such as student loans).
    /// </summary>
    public interface ILoanProvider
    {
        /// <summary>
        /// Asks a lender for terms.
        /// </summary>
        /// <param name="productId">Loan product.</param>
        /// <param name="amount">Amount to borrow.</param>
        /// <returns>The quote.</returns>
        LoanQuote Quote(string productId, decimal amount);

        /// <summary>
        /// Takes an unsecured installment loan and receives the cash.
        /// </summary>
        /// <param name="productId">Loan product.</param>
        /// <param name="amount">Amount.</param>
        /// <returns>The loan.</returns>
        Debt TakeLoan(string productId, decimal amount);
    }
}
