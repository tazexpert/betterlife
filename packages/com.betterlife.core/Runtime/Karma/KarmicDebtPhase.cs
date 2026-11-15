using System;
using BetterLife.Core.Events;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Delayed-consequences tick step: publishes every karmic debt that has come due.
    /// </summary>
    public sealed class KarmicDebtPhase : ITickPhase
    {
        private readonly IEventBus _bus;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="bus">Event bus.</param>
        public KarmicDebtPhase(IEventBus bus)
        {
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc />
        public string Name => "karmic_debt";

        /// <inheritdoc />
        public int Order => TickOrder.DelayedConsequences;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Weekly;

        /// <inheritdoc />
        public void Execute(TickContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            foreach (KarmicDebt debt in context.Character.Karma.TakeDue(context.Week))
            {
                _bus.Publish(new KarmicDebtDueEvent(debt));
            }
        }
    }
}
