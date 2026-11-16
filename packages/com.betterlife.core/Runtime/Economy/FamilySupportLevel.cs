using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// What a family of one household income level gives a dependent child: pocket money by age band and a gift at independence.
    /// </summary>
    public sealed class FamilySupportLevel
    {
        /// <summary>
        /// Creates the level.
        /// </summary>
        /// <param name="incomeLevel">Household income level (1 = poorest).</param>
        /// <param name="allowances">Pocket money by age band; ages must increase.</param>
        /// <param name="independenceGift">Gift when the child becomes independent; non-negative.</param>
        public FamilySupportLevel(int incomeLevel, IEnumerable<AllowanceBand> allowances, decimal independenceGift)
        {
            if (incomeLevel < 1 || independenceGift < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(incomeLevel), "Level must be at least 1 and the gift non-negative.");
            }
            List<AllowanceBand> bands = (allowances ?? throw new ArgumentNullException(nameof(allowances))).ToList();
            for (int i = 1; i < bands.Count; i++)
            {
                if (bands[i].FromAge <= bands[i - 1].FromAge)
                {
                    throw new ArgumentException("Allowance bands must be in increasing age order.", nameof(allowances));
                }
            }
            IncomeLevel = incomeLevel;
            Allowances = bands;
            IndependenceGift = independenceGift;
        }

        /// <summary>Household income level (1 = poorest).</summary>
        public int IncomeLevel { get; }

        /// <summary>Pocket money by age band, youngest first.</summary>
        public IReadOnlyList<AllowanceBand> Allowances { get; }

        /// <summary>Gift when the child becomes independent.</summary>
        public decimal IndependenceGift { get; }

        /// <summary>
        /// Monthly pocket money at an age (the latest band that has started; 0 before the first).
        /// </summary>
        /// <param name="age">Age in years.</param>
        /// <returns>The allowance.</returns>
        public decimal AllowanceAt(int age) => Allowances.LastOrDefault(b => b.FromAge <= age)?.Monthly ?? 0m;
    }
}
