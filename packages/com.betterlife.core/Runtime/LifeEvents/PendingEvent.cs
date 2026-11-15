using BetterLife.Core.Time;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// An event in the inbox waiting for the player's choice.
    /// </summary>
    public sealed class PendingEvent
    {
        /// <summary>
        /// Creates a pending event.
        /// </summary>
        /// <param name="instanceId">Unique id of this occurrence.</param>
        /// <param name="eventId">Event id.</param>
        /// <param name="origin">Why it occurred.</param>
        /// <param name="firedOn">Week it occurred.</param>
        /// <param name="deadline">Week the default choice is applied if the player has not answered.</param>
        public PendingEvent(int instanceId, string eventId, EventOrigin origin, GameDate firedOn, GameDate deadline)
        {
            InstanceId = instanceId;
            EventId = eventId;
            Origin = origin;
            FiredOn = firedOn;
            Deadline = deadline;
        }

        /// <summary>Unique id of this occurrence.</summary>
        public int InstanceId { get; }

        /// <summary>Event id.</summary>
        public string EventId { get; }

        /// <summary>Why it occurred.</summary>
        public EventOrigin Origin { get; }

        /// <summary>Week it occurred.</summary>
        public GameDate FiredOn { get; }

        /// <summary>Week the default choice is applied.</summary>
        public GameDate Deadline { get; }
    }
}
