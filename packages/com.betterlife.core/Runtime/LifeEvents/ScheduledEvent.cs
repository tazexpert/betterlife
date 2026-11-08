using BetterLife.Core.Time;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// An event set to occur in a future week (delayed consequence).
    /// </summary>
    public sealed class ScheduledEvent
    {
        /// <summary>
        /// Creates a scheduled event.
        /// </summary>
        /// <param name="eventId">Event id.</param>
        /// <param name="dueOn">Week it occurs.</param>
        public ScheduledEvent(string eventId, GameDate dueOn)
        {
            EventId = eventId;
            DueOn = dueOn;
        }

        /// <summary>Event id.</summary>
        public string EventId { get; }

        /// <summary>Week it occurs.</summary>
        public GameDate DueOn { get; }
    }
}
