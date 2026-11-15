using System;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// What a family of a given income level gives a child: a monthly allowance and a gift at independence.
    /// </summary>
    public sealed class FamilySupportLevel
    {
        /// <summary>
        /// Creates a level.
        /// </summary>
        /// <param name="incomeLevel">Household income level (matches family backgrounds).</param>
        /// <param name="monthlyAllowance">Monthly allowance; non-negative.</param>
        /// <param name="independenceGift">Gift on becoming independent; non-negative.</param>
        public FamilySupportLevel(int incomeLevel, decimal monthlyAllowance, decimal independenceGift)
        {
            if (incomeLevel < 1 || monthlyAllowance < 0m || independenceGift < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(incomeLevel), "Level must be at least 1 and amounts non-negative.");
            }
            IncomeLevel = incomeLevel;
            MonthlyAllowance = monthlyAllowance;
            IndependenceGift = independenceGift;
        }

        /// <summary>Household income level.</summary>
        public int IncomeLevel { get; }

        /// <summary>Monthly allowance.</summary>
        public decimal MonthlyAllowance { get; }

        /// <summary>Gift on becoming independent.</summary>
        public decimal IndependenceGift { get; }
    }
}
