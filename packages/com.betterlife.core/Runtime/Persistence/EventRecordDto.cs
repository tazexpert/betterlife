using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of an event record.</summary>
    [Serializable]
    public sealed class EventRecordDto
    {
        /// <summary>Event id.</summary>
        public string EventId;

        /// <summary>Latest occurrence week.</summary>
        public int LastFiredWeek;

        /// <summary>Occurrences.</summary>
        public int Occurrences;
    }
}
