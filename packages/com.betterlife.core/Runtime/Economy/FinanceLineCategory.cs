namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Category of a line in the monthly finance report.
    /// </summary>
    public enum FinanceLineCategory
    {
        /// <summary>Earned or received money.</summary>
        Income = 0,
        /// <summary>Income tax.</summary>
        Tax = 1,
        /// <summary>Lifestyle costs.</summary>
        Living = 2,
        /// <summary>Loan repayment.</summary>
        DebtPayment = 3,
        /// <summary>Interest and fees added to loans.</summary>
        Interest = 4,
        /// <summary>Change in asset values.</summary>
        Investment = 5,
        /// <summary>Loss of cash value to inflation.</summary>
        Inflation = 6,
        /// <summary>Money paid on the character's behalf (family) or borrowed to cover a shortfall.</summary>
        Shortfall = 7,
    }
}
