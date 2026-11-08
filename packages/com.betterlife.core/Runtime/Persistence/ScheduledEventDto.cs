using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a scheduled event.</summary>
    [Serializable]
    public sealed class ScheduledEventDto
    {
        /// <summary>Event id.</summary>
        public string EventId;

        /// <summary>Due week.</summary>
        public int DueWeek;
    }
}
