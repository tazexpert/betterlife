using System;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Monthly finance step (order 250, after time allocation and decay).
    /// </summary>
    public sealed class FinanceMonthPhase : ITickPhase
    {
        private readonly FinanceService _finance;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="finance">Finance service.</param>
        public FinanceMonthPhase(FinanceService finance)
        {
            _finance = finance ?? throw new ArgumentNullException(nameof(finance));
        }

        /// <inheritdoc />
        public string Name => "finance_month";

        /// <inheritdoc />
        public int Order => TickOrder.NaturalDecay + 50;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Monthly;

        /// <inheritdoc />
        public void Execute(TickContext context) => _finance.ProcessMonth(context);
    }
}
