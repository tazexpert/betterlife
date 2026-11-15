using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Assets, loans and lifestyle of one character (cash stays in the wallet).
    /// </summary>
    public sealed class CharacterFinances
    {
        /// <summary>Assets.</summary>
        public Portfolio Portfolio { get; } = new Portfolio();

        /// <summary>Loans.</summary>
        public DebtBook Debts { get; } = new DebtBook();

        /// <summary>Chosen lifestyle tier, or null while living with parents.</summary>
        public string LifestyleTierId { get; private set; }

        /// <summary>Week the bankruptcy period ends, or null when never bankrupt.</summary>
        public GameDate? BankruptUntil { get; private set; }

        /// <summary>Gross income of the latest month (used for credit limits and loan checks).</summary>
        public decimal LastMonthlyIncome { get; private set; }

        /// <summary>
        /// Records the latest month's gross income.
        /// </summary>
        /// <param name="amount">Non-negative amount.</param>
        public void SetLastMonthlyIncome(decimal amount) => LastMonthlyIncome = System.Math.Max(0m, amount);

        /// <summary>
        /// Returns true while the bankruptcy period lasts.
        /// </summary>
        /// <param name="now">Current week.</param>
        /// <returns>True when bankrupt.</returns>
        public bool IsBankrupt(GameDate now) => BankruptUntil.HasValue && now < BankruptUntil.Value;

        /// <summary>
        /// Net worth for a given cash amount: cash + assets − debts.
        /// </summary>
        /// <param name="cash">Cash on hand.</param>
        /// <returns>Net worth.</returns>
        public decimal NetWorth(decimal cash) => cash + Portfolio.TotalValue - Debts.TotalBalance;

        /// <summary>
        /// Sets the lifestyle tier.
        /// </summary>
        /// <param name="tierId">Tier id.</param>
        public void SetLifestyle(string tierId) => LifestyleTierId = ContentId.Require(tierId, nameof(tierId));

        /// <summary>
        /// Starts or restores a bankruptcy period.
        /// </summary>
        /// <param name="until">End week, or null to clear.</param>
        public void SetBankruptUntil(GameDate? until) => BankruptUntil = until;

        /// <summary>
        /// Restores the lifestyle (null while living with parents).
        /// </summary>
        /// <param name="tierId">Tier id or null.</param>
        public void RestoreLifestyle(string tierId) => LifestyleTierId = tierId == null ? null : ContentId.Require(tierId, nameof(tierId));
    }
}
