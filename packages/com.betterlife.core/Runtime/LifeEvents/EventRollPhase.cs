using System;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Event tick step (order 400): runs the week's events after delayed consequences such as karmic debts.
    /// </summary>
    public sealed class EventRollPhase : ITickPhase
    {
        private readonly EventSystem _events;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="events">Event system.</param>
        public EventRollPhase(EventSystem events)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
        }

        /// <inheritdoc />
        public string Name => "event_roll";

        /// <inheritdoc />
        public int Order => TickOrder.EventRoll;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Weekly;

        /// <inheritdoc />
        public void Execute(TickContext context) => _events.RunWeek(context);
    }
}
