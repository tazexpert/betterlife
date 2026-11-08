using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Personal finance rules from content/finance_settings.json.
    /// </summary>
    public sealed class FinanceSettings
    {
        /// <summary>
        /// Creates settings.
        /// </summary>
        /// <param name="independenceAge">Age at which the character pays their own way.</param>
        /// <param name="allowanceStartAge">Age from which the family pays an allowance.</param>
        /// <param name="defaultLifestyleId">Lifestyle chosen at independence.</param>
        /// <param name="referenceLifestyleId">Lifestyle whose cost measures wealth and debt stress.</param>
        /// <param name="familySupport">Support per household income level.</param>
        /// <param name="fallbackProductId">Loan product that covers shortfalls when no card has room (loan shark).</param>
        /// <param name="defaultAfterMissed">Consecutive missed payments that make a loan default; at least 1.</param>
        /// <param name="forcedSaleDiscount">Value lost when collateral is seized or assets are liquidated, 0-1.</param>
        /// <param name="credit">Credit score rules.</param>
        /// <param name="bankruptcyNetWorth">Net worth below which a defaulted borrower goes bankrupt (negative).</param>
        /// <param name="bankruptcyWeeks">How long bankruptcy blocks formal credit; positive.</param>
        /// <param name="debtStressPerYearOfCosts">Monthly stress per year of reference costs owed; non-negative.</param>
        /// <param name="debtStressMax">Cap of monthly debt stress; non-negative.</param>
        /// <param name="informalDebtStress">Extra monthly stress while owing a loan shark; non-negative.</param>
        /// <param name="wealthCurve">Wealth score curve.</param>
        public FinanceSettings(int independenceAge, int allowanceStartAge, string defaultLifestyleId, string referenceLifestyleId,
            IEnumerable<FamilySupportLevel> familySupport, string fallbackProductId, int defaultAfterMissed, decimal forcedSaleDiscount,
            CreditScoreRules credit, decimal bankruptcyNetWorth, int bankruptcyWeeks, float debtStressPerYearOfCosts, float debtStressMax,
            float informalDebtStress, WealthCurve wealthCurve)
        {
            if (independenceAge < 1 || allowanceStartAge < 0 || allowanceStartAge > independenceAge || defaultAfterMissed < 1 || bankruptcyWeeks < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(independenceAge), "Ages and counts are out of range.");
            }
            if (forcedSaleDiscount < 0m || forcedSaleDiscount >= 1m || bankruptcyNetWorth > 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(forcedSaleDiscount), "Discount must be within 0-1 and the bankruptcy threshold non-positive.");
            }
            if (!(debtStressPerYearOfCosts >= 0f) || !(debtStressMax >= 0f) || !(informalDebtStress >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(debtStressMax), "Stress values must be non-negative.");
            }
            IndependenceAge = independenceAge;
            AllowanceStartAge = allowanceStartAge;
            DefaultLifestyleId = ContentId.Require(defaultLifestyleId, nameof(defaultLifestyleId));
            ReferenceLifestyleId = ContentId.Require(referenceLifestyleId, nameof(referenceLifestyleId));
            FamilySupport = (familySupport ?? Enumerable.Empty<FamilySupportLevel>()).ToDictionary(l => l.IncomeLevel);
            FallbackProductId = ContentId.Require(fallbackProductId, nameof(fallbackProductId));
            DefaultAfterMissed = defaultAfterMissed;
            ForcedSaleDiscount = forcedSaleDiscount;
            Credit = credit ?? throw new ArgumentNullException(nameof(credit));
            BankruptcyNetWorth = bankruptcyNetWorth;
            BankruptcyWeeks = bankruptcyWeeks;
            DebtStressPerYearOfCosts = debtStressPerYearOfCosts;
            DebtStressMax = debtStressMax;
            InformalDebtStress = informalDebtStress;
            WealthCurve = wealthCurve ?? throw new ArgumentNullException(nameof(wealthCurve));
        }

        /// <summary>Age of independence.</summary>
        public int IndependenceAge { get; }

        /// <summary>Age the allowance starts.</summary>
        public int AllowanceStartAge { get; }

        /// <summary>Lifestyle chosen at independence.</summary>
        public string DefaultLifestyleId { get; }

        /// <summary>Lifestyle used as the cost yardstick.</summary>
        public string ReferenceLifestyleId { get; }

        /// <summary>Support per household income level.</summary>
        public IReadOnlyDictionary<int, FamilySupportLevel> FamilySupport { get; }

        /// <summary>Loan product covering shortfalls.</summary>
        public string FallbackProductId { get; }

        /// <summary>Missed payments before default.</summary>
        public int DefaultAfterMissed { get; }

        /// <summary>Value lost in forced sales.</summary>
        public decimal ForcedSaleDiscount { get; }

        /// <summary>Credit score rules.</summary>
        public CreditScoreRules Credit { get; }

        /// <summary>Bankruptcy net worth threshold.</summary>
        public decimal BankruptcyNetWorth { get; }

        /// <summary>Length of bankruptcy in weeks.</summary>
        public int BankruptcyWeeks { get; }

        /// <summary>Monthly stress per year of costs owed.</summary>
        public float DebtStressPerYearOfCosts { get; }

        /// <summary>Cap of monthly debt stress.</summary>
        public float DebtStressMax { get; }

        /// <summary>Extra monthly stress with loan shark debt.</summary>
        public float InformalDebtStress { get; }

        /// <summary>Wealth score curve.</summary>
        public WealthCurve WealthCurve { get; }
    }
}
