using System;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// One payment from an income source.
    /// </summary>
    public sealed class IncomeItem
    {
        /// <summary>
        /// Creates a payment.
        /// </summary>
        /// <param name="sourceKey">Localization key of the source.</param>
        /// <param name="amount">Gross amount; positive.</param>
        /// <param name="taxable">Whether income tax applies.</param>
        public IncomeItem(string sourceKey, decimal amount, bool taxable)
        {
            if (amount <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Income must be positive.");
            }
            SourceKey = ContentId.RequireLocKey(sourceKey, nameof(sourceKey));
            Amount = amount;
            Taxable = taxable;
        }

        /// <summary>Localization key of the source.</summary>
        public string SourceKey { get; }

        /// <summary>Gross amount.</summary>
        public decimal Amount { get; }

        /// <summary>Whether income tax applies.</summary>
        public bool Taxable { get; }
    }
}
