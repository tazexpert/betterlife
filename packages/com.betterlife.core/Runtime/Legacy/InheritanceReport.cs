namespace BetterLife.Core.Legacy
{
    /// <summary>
    /// How an estate was settled for the heir (real money).
    /// </summary>
    public sealed class InheritanceReport
    {
        /// <summary>
        /// Creates a report.
        /// </summary>
        /// <param name="assets">Cash plus holdings of the deceased.</param>
        /// <param name="debts">Debts paid from the estate.</param>
        /// <param name="tax">Inheritance tax.</param>
        /// <param name="heirs">Living children sharing the estate.</param>
        /// <param name="received">What this heir received.</param>
        public InheritanceReport(decimal assets, decimal debts, decimal tax, int heirs, decimal received)
        {
            Assets = assets;
            Debts = debts;
            Tax = tax;
            Heirs = heirs;
            Received = received;
        }

        /// <summary>Cash plus holdings of the deceased.</summary>
        public decimal Assets { get; }

        /// <summary>Debts paid from the estate (never more than the assets).</summary>
        public decimal Debts { get; }

        /// <summary>Inheritance tax.</summary>
        public decimal Tax { get; }

        /// <summary>Living children sharing the estate.</summary>
        public int Heirs { get; }

        /// <summary>What this heir received.</summary>
        public decimal Received { get; }

        /// <summary>Estate after debts (never negative: heirs do not take on debts beyond the estate).</summary>
        public decimal NetEstate => System.Math.Max(0m, Assets - Debts);

        /// <summary>
        /// Settles an estate.
        /// </summary>
        /// <param name="assets">Cash plus holdings (real).</param>
        /// <param name="debts">Debts (real).</param>
        /// <param name="heirs">Living children (at least 1).</param>
        /// <param name="settings">Tax rules.</param>
        /// <returns>The report.</returns>
        public static InheritanceReport Settle(decimal assets, decimal debts, int heirs, LegacySettings settings)
        {
            if (settings == null)
            {
                throw new System.ArgumentNullException(nameof(settings));
            }
            if (heirs < 1)
            {
                throw new System.ArgumentOutOfRangeException(nameof(heirs), heirs, "An estate needs at least one heir.");
            }
            decimal paid = System.Math.Min(System.Math.Max(0m, debts), System.Math.Max(0m, assets));
            decimal net = System.Math.Max(0m, assets - paid);
            decimal tax = Economy.Money.Round(System.Math.Max(0m, net - settings.TaxExemption) * settings.TaxRate);
            decimal received = Economy.Money.Round((net - tax) / heirs);
            return new InheritanceReport(assets, paid, tax, heirs, received);
        }
    }
}
