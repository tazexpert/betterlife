using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a loan.</summary>
    [Serializable]
    public sealed class DebtDto
    {
        /// <summary>Loan id.</summary>
        public int Id;

        /// <summary>Loan product id.</summary>
        public string ProductId;

        /// <summary>Debt kind (enum name).</summary>
        public string Kind;

        /// <summary>Balance as an invariant-culture decimal string.</summary>
        public string Balance;

        /// <summary>Annual interest rate.</summary>
        public float AnnualRate;

        /// <summary>Installment as an invariant-culture decimal string.</summary>
        public string MonthlyPayment;

        /// <summary>Installments left.</summary>
        public int MonthsRemaining;

        /// <summary>Revolving limit as an invariant-culture decimal string.</summary>
        public string CreditLimit;

        /// <summary>Whether CollateralHoldingId is meaningful.</summary>
        public bool HasCollateral;

        /// <summary>Holding securing the loan when HasCollateral.</summary>
        public int CollateralHoldingId;

        /// <summary>Missed payments in a row.</summary>
        public int MissedPayments;

        /// <summary>Whether the loan is in default.</summary>
        public bool Defaulted;

        /// <summary>Start week (GameDate.TotalWeeks).</summary>
        public int StartedWeek;
    }
}
