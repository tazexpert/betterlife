using System;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// Weekly vices, after delayed consequences and before the event roll.
    /// </summary>
    public sealed class ViceWeekPhase : ITickPhase
    {
        private const int OrderAfterDelayedConsequences = 50;

        private readonly ViceService _vices;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="vices">Vice service.</param>
        public ViceWeekPhase(ViceService vices)
        {
            _vices = vices ?? throw new ArgumentNullException(nameof(vices));
        }

        /// <inheritdoc />
        public string Name => "vice_week";

        /// <inheritdoc />
        public int Order => TickOrder.DelayedConsequences + OrderAfterDelayedConsequences;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Weekly;

        /// <inheritdoc />
        public void Execute(TickContext context) => _vices.ProcessWeek(context);
    }
}
