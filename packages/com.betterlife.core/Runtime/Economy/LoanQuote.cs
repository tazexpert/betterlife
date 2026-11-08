using System.Collections.Generic;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// The terms a lender offers, or why it refuses.
    /// </summary>
    public sealed class LoanQuote
    {
        /// <summary>Too young to borrow.</summary>
        public const string ReasonTooYoung = "finance.loan.too_young";
        /// <summary>Bankruptcy blocks formal credit.</summary>
        public const string ReasonBankrupt = "finance.loan.bankrupt";
        /// <summary>Credit score below the product's minimum.</summary>
        public const string ReasonCreditScore = "finance.loan.credit_score";
        /// <summary>Amount too large for the income.</summary>
        public const string ReasonIncome = "finance.loan.income";
        /// <summary>The product needs a different collateral.</summary>
        public const string ReasonCollateral = "finance.loan.collateral";
        /// <summary>Not enough cash for the down payment or purchase.</summary>
        public const string ReasonCash = "finance.loan.cash";
        /// <summary>Amount below the minimum.</summary>
        public const string ReasonAmount = "finance.loan.amount";

        /// <summary>
        /// Creates a quote.
        /// </summary>
        /// <param name="reasonKeys">Refusal reasons (empty when approved).</param>
        /// <param name="annualRate">Nominal annual rate.</param>
        /// <param name="monthlyPayment">Installment (0 for revolving).</param>
        /// <param name="creditLimit">Revolving limit (0 for installment loans).</param>
        public LoanQuote(IReadOnlyList<string> reasonKeys, float annualRate, decimal monthlyPayment, decimal creditLimit)
        {
            ReasonKeys = reasonKeys ?? new string[0];
            AnnualRate = annualRate;
            MonthlyPayment = monthlyPayment;
            CreditLimit = creditLimit;
        }

        /// <summary>True when the lender agrees.</summary>
        public bool Approved => ReasonKeys.Count == 0;

        /// <summary>Refusal reasons.</summary>
        public IReadOnlyList<string> ReasonKeys { get; }

        /// <summary>Nominal annual rate.</summary>
        public float AnnualRate { get; }

        /// <summary>Installment.</summary>
        public decimal MonthlyPayment { get; }

        /// <summary>Revolving limit.</summary>
        public decimal CreditLimit { get; }
    }
}
