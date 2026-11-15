using BetterLife.Core.Time;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// How often and how recently an event has occurred in this life (for cooldowns and limits).
    /// </summary>
    public sealed class EventRecord
    {
        /// <summary>
        /// Creates a record.
        /// </summary>
        /// <param name="eventId">Event id.</param>
        /// <param name="lastFiredOn">Week of the latest occurrence.</param>
        /// <param name="occurrences">Number of occurrences; at least 1.</param>
        public EventRecord(string eventId, GameDate lastFiredOn, int occurrences)
        {
            EventId = eventId;
            LastFiredOn = lastFiredOn;
            Occurrences = occurrences;
        }

        /// <summary>Event id.</summary>
        public string EventId { get; }

        /// <summary>Week of the latest occurrence.</summary>
        public GameDate LastFiredOn { get; private set; }

        /// <summary>Number of occurrences.</summary>
        public int Occurrences { get; private set; }

        internal void RecordFire(GameDate week)
        {
            LastFiredOn = week;
            Occurrences++;
        }
    }
}
