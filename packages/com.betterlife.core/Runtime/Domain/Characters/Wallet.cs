using System;

namespace BetterLife.Core.Domain.Characters
{
    /// <summary>
    /// Cash on hand. Assets and debts are added by the Economy task.
    /// </summary>
    public sealed class Wallet
    {
        /// <summary>
        /// Creates a wallet.
        /// </summary>
        /// <param name="cash">Starting cash; must be non-negative.</param>
        public Wallet(decimal cash = 0m)
        {
            if (cash < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(cash), cash, "Cash must be non-negative.");
            }
            Cash = cash;
        }

        /// <summary>Cash on hand.</summary>
        public decimal Cash { get; private set; }

        /// <summary>
        /// Adds cash.
        /// </summary>
        /// <param name="amount">Positive amount.</param>
        public void Deposit(decimal amount)
        {
            RequirePositive(amount);
            Cash += amount;
        }

        /// <summary>
        /// Removes cash if enough is available; otherwise leaves the wallet unchanged.
        /// </summary>
        /// <param name="amount">Positive amount.</param>
        /// <returns>True when withdrawn.</returns>
        public bool TryWithdraw(decimal amount)
        {
            RequirePositive(amount);
            if (amount > Cash)
            {
                return false;
            }
            Cash -= amount;
            return true;
        }

        private static void RequirePositive(decimal amount)
        {
            if (amount <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be positive.");
            }
        }
    }
}
