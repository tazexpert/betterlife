using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// A loan product, loaded from content/debts.json.
    /// </summary>
    public sealed class DebtProductDefinition : IDefinition
    {
        /// <summary>
        /// Creates a product.
        /// </summary>
        /// <param name="id">Product id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="kind">Kind of debt.</param>
        /// <param name="annualRate">Fixed nominal annual rate, or null to use the policy rate plus <paramref name="rateSpread"/>.</param>
        /// <param name="rateSpread">Spread over the policy rate when no fixed rate is given.</param>
        /// <param name="termMonths">Installment term; 0 for revolving credit.</param>
        /// <param name="minPaymentRate">Revolving: minimum payment as a share of the balance, 0-1.</param>
        /// <param name="minPayment">Revolving: minimum payment amount; non-negative.</param>
        /// <param name="limitIncomeMultiple">Revolving: credit limit as a multiple of monthly income.</param>
        /// <param name="minCreditScore">Required credit score (0-100).</param>
        /// <param name="maxIncomeMultiple">Largest loan as a multiple of annual income, or null for no income check.</param>
        /// <param name="collateralClass">Asset class securing the loan, or null for unsecured.</param>
        /// <param name="downPaymentRate">Share of the price paid upfront for secured purchases, 0-1.</param>
        /// <param name="dischargeable">Whether bankruptcy erases it.</param>
        /// <param name="graceMonths">Months before payments start.</param>
        /// <param name="lateFee">Fee added on a missed payment; non-negative.</param>
        public DebtProductDefinition(string id, string nameKey, DebtKind kind, float? annualRate, float? rateSpread, int termMonths,
            float minPaymentRate, decimal minPayment, float limitIncomeMultiple, float minCreditScore, float? maxIncomeMultiple,
            AssetClass? collateralClass, float downPaymentRate, bool dischargeable, int graceMonths, decimal lateFee)
        {
            if (annualRate.HasValue == rateSpread.HasValue)
            {
                throw new ArgumentException("Give exactly one of a fixed rate or a spread over the policy rate.", nameof(annualRate));
            }
            if (termMonths < 0 || graceMonths < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(termMonths), termMonths, "Months must be non-negative.");
            }
            if (!(minPaymentRate >= 0f) || minPaymentRate > 1f || !(downPaymentRate >= 0f) || downPaymentRate >= 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(minPaymentRate), "Rates must be within 0-1 (down payment below 1).");
            }
            if (minPayment < 0m || lateFee < 0m || !(limitIncomeMultiple >= 0f) || !StatBounds.IsInRange(minCreditScore))
            {
                throw new ArgumentOutOfRangeException(nameof(minPayment), "Amounts must be non-negative and the score within 0-100.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Kind = kind;
            AnnualRate = annualRate;
            RateSpread = rateSpread;
            TermMonths = termMonths;
            MinPaymentRate = minPaymentRate;
            MinPayment = minPayment;
            LimitIncomeMultiple = limitIncomeMultiple;
            MinCreditScore = minCreditScore;
            MaxIncomeMultiple = maxIncomeMultiple;
            CollateralClass = collateralClass;
            DownPaymentRate = downPaymentRate;
            Dischargeable = dischargeable;
            GraceMonths = graceMonths;
            LateFee = lateFee;
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Kind of debt.</summary>
        public DebtKind Kind { get; }

        /// <summary>Fixed nominal annual rate, or null.</summary>
        public float? AnnualRate { get; }

        /// <summary>Spread over the policy rate, or null.</summary>
        public float? RateSpread { get; }

        /// <summary>Installment term; 0 for revolving.</summary>
        public int TermMonths { get; }

        /// <summary>True for revolving credit.</summary>
        public bool IsRevolving => TermMonths == 0;

        /// <summary>Revolving minimum payment share.</summary>
        public float MinPaymentRate { get; }

        /// <summary>Revolving minimum payment amount.</summary>
        public decimal MinPayment { get; }

        /// <summary>Revolving limit as a multiple of monthly income.</summary>
        public float LimitIncomeMultiple { get; }

        /// <summary>Required credit score.</summary>
        public float MinCreditScore { get; }

        /// <summary>Largest loan as a multiple of annual income, or null.</summary>
        public float? MaxIncomeMultiple { get; }

        /// <summary>Collateral asset class, or null.</summary>
        public AssetClass? CollateralClass { get; }

        /// <summary>Upfront share for secured purchases.</summary>
        public float DownPaymentRate { get; }

        /// <summary>Whether bankruptcy erases it.</summary>
        public bool Dischargeable { get; }

        /// <summary>Months before payments start.</summary>
        public int GraceMonths { get; }

        /// <summary>Fee on a missed payment.</summary>
        public decimal LateFee { get; }

        /// <summary>
        /// Returns the nominal annual rate for a loan taken now.
        /// </summary>
        /// <param name="policyRate">Current policy rate.</param>
        /// <returns>Annual rate.</returns>
        public float RateAt(float policyRate) => AnnualRate ?? Math.Max(0f, policyRate + RateSpread.Value);
    }
}
