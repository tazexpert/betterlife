using System;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// Monthly scams, right after the finance month has paid income.
    /// </summary>
    public sealed class ScamMonthPhase : ITickPhase
    {
        private const int OrderAfterDecay = 60;

        private readonly ScamService _scams;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="scams">Scam service.</param>
        public ScamMonthPhase(ScamService scams)
        {
            _scams = scams ?? throw new ArgumentNullException(nameof(scams));
        }

        /// <inheritdoc />
        public string Name => "scam_month";

        /// <inheritdoc />
        public int Order => TickOrder.NaturalDecay + OrderAfterDecay;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Monthly;

        /// <inheritdoc />
        public void Execute(TickContext context) => _scams.ProcessMonth(context);
    }
}
