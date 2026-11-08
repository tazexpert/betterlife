using System;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Yearly step (order 60): closes the economic year.
    /// </summary>
    public sealed class EconomyYearPhase : ITickPhase
    {
        private readonly EconomyService _economy;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="economy">Economy service.</param>
        public EconomyYearPhase(EconomyService economy)
        {
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
        }

        /// <inheritdoc />
        public string Name => "economy_year";

        /// <inheritdoc />
        public int Order => TickOrder.TimeAllocation - 40;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Yearly;

        /// <inheritdoc />
        public void Execute(TickContext context) => _economy.AdvanceYear();
    }
}
