using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// One loan of the character. The balance is real base-year baht (inflation shrinks it every month).
    /// </summary>
    public sealed class Debt
    {
        /// <summary>
        /// Creates a debt.
        /// </summary>
        /// <param name="id">Unique id; positive.</param>
        /// <param name="productId">Product id.</param>
        /// <param name="kind">Kind of debt (copied from the product).</param>
        /// <param name="balance">Amount owed; non-negative.</param>
        /// <param name="annualRate">Nominal annual rate.</param>
        /// <param name="monthlyPayment">Installment (0 for revolving).</param>
        /// <param name="monthsRemaining">Installments left (0 for revolving).</param>
        /// <param name="creditLimit">Revolving limit (0 for installment loans).</param>
        /// <param name="collateralHoldingId">Holding securing the loan, or null.</param>
        /// <param name="startedOn">Week the loan started.</param>
        public Debt(int id, string productId, DebtKind kind, decimal balance, float annualRate, decimal monthlyPayment, int monthsRemaining, decimal creditLimit,
            int? collateralHoldingId, GameDate startedOn)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, "Id must be positive.");
            }
            if (balance < 0m || monthlyPayment < 0m || creditLimit < 0m || monthsRemaining < 0 || !(annualRate >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(balance), "Loan values must be non-negative.");
            }
            Id = id;
            ProductId = ContentId.Require(productId, nameof(productId));
            Kind = kind;
            Balance = balance;
            AnnualRate = annualRate;
            MonthlyPayment = monthlyPayment;
            MonthsRemaining = monthsRemaining;
            CreditLimit = creditLimit;
            CollateralHoldingId = collateralHoldingId;
            StartedOn = startedOn;
        }

        /// <summary>Unique id.</summary>
        public int Id { get; }

        /// <summary>Product id.</summary>
        public string ProductId { get; }

        /// <summary>Kind of debt.</summary>
        public DebtKind Kind { get; }

        /// <summary>Amount owed.</summary>
        public decimal Balance { get; private set; }

        /// <summary>Nominal annual rate.</summary>
        public float AnnualRate { get; }

        /// <summary>Installment (0 for revolving).</summary>
        public decimal MonthlyPayment { get; }

        /// <summary>Installments left.</summary>
        public int MonthsRemaining { get; private set; }

        /// <summary>Revolving limit.</summary>
        public decimal CreditLimit { get; }

        /// <summary>Unused revolving credit.</summary>
        public decimal AvailableCredit => Math.Max(0m, CreditLimit - Balance);

        /// <summary>Holding securing the loan, or null.</summary>
        public int? CollateralHoldingId { get; private set; }

        /// <summary>Consecutive missed payments.</summary>
        public int MissedPayments { get; private set; }

        /// <summary>True after too many missed payments.</summary>
        public bool IsDefaulted { get; private set; }

        /// <summary>Week the loan started.</summary>
        public GameDate StartedOn { get; }

        internal void Add(decimal amount, decimal cap) => Balance = Math.Min(cap, Money.Round(Balance + amount));

        internal void Pay(decimal amount)
        {
            Balance = Math.Max(0m, Money.Round(Balance - amount));
        }

        internal void Deflate(decimal monthlyInflation) => Balance = Money.Round(Balance / (1m + monthlyInflation));

        internal void CountInstallment()
        {
            MonthsRemaining = Math.Max(0, MonthsRemaining - 1);
            MissedPayments = 0;
        }

        internal void Miss() => MissedPayments++;

        internal void MarkDefaulted() => IsDefaulted = true;

        internal void ReleaseCollateral() => CollateralHoldingId = null;

        internal void RestoreStatus(int missed, bool defaulted)
        {
            MissedPayments = Math.Max(0, missed);
            IsDefaulted = defaulted;
        }
    }
}
