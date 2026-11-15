using System;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// How much a successful scam takes.
    /// </summary>
    public sealed class ScamLoss
    {
        /// <summary>
        /// Creates the value.
        /// </summary>
        /// <param name="cashShare">Share of cash taken, 0-1.</param>
        /// <param name="savingsShare">Share of deposit savings taken, 0-1.</param>
        /// <param name="min">Smallest loss (limited to what the character has).</param>
        /// <param name="max">Largest loss.</param>
        public ScamLoss(decimal cashShare, decimal savingsShare, decimal min, decimal max)
        {
            if (cashShare < 0m || cashShare > 1m || savingsShare < 0m || savingsShare > 1m || min < 0m || max < min)
            {
                throw new ArgumentOutOfRangeException(nameof(cashShare), "Values are out of range.");
            }
            CashShare = cashShare;
            SavingsShare = savingsShare;
            Min = min;
            Max = max;
        }

        /// <summary>Share of cash taken, 0-1.</summary>
        public decimal CashShare { get; }

        /// <summary>Share of deposit savings taken, 0-1.</summary>
        public decimal SavingsShare { get; }

        /// <summary>Smallest loss (limited to what the character has).</summary>
        public decimal Min { get; }

        /// <summary>Largest loss.</summary>
        public decimal Max { get; }
    }
}
