using BetterLife.Core.Time;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// A finished occurrence, for the timeline and the life summary.
    /// </summary>
    public sealed class EventHistoryEntry
    {
        /// <summary>
        /// Creates an entry.
        /// </summary>
        /// <param name="instanceId">Occurrence id.</param>
        /// <param name="eventId">Event id.</param>
        /// <param name="origin">Why it occurred.</param>
        /// <param name="week">Week it was resolved.</param>
        /// <param name="choiceId">Chosen option, or null for announcements and dismissed events.</param>
        /// <param name="succeeded">Whether the outcome was a success.</param>
        public EventHistoryEntry(int instanceId, string eventId, EventOrigin origin, GameDate week, string choiceId, bool succeeded)
        {
            InstanceId = instanceId;
            EventId = eventId;
            Origin = origin;
            Week = week;
            ChoiceId = choiceId;
            Succeeded = succeeded;
        }

        /// <summary>Occurrence id.</summary>
        public int InstanceId { get; }

        /// <summary>Event id.</summary>
        public string EventId { get; }

        /// <summary>Why it occurred.</summary>
        public EventOrigin Origin { get; }

        /// <summary>Week it was resolved.</summary>
        public GameDate Week { get; }

        /// <summary>Chosen option, or null.</summary>
        public string ChoiceId { get; }

        /// <summary>Whether the outcome was a success.</summary>
        public bool Succeeded { get; }
    }
}
