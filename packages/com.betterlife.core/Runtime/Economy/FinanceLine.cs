namespace BetterLife.Core.Economy
{
    /// <summary>
    /// One line of the monthly finance report (positive adds to the character, negative takes away).
    /// </summary>
    public sealed class FinanceLine
    {
        /// <summary>
        /// Creates a line.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <param name="sourceKey">Localization key of the source.</param>
        /// <param name="amount">Signed real amount.</param>
        public FinanceLine(FinanceLineCategory category, string sourceKey, decimal amount)
        {
            Category = category;
            SourceKey = sourceKey;
            Amount = amount;
        }

        /// <summary>Category.</summary>
        public FinanceLineCategory Category { get; }

        /// <summary>Localization key of the source.</summary>
        public string SourceKey { get; }

        /// <summary>Signed real amount.</summary>
        public decimal Amount { get; }
    }
}
