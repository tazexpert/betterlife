using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of an inbox entry.</summary>
    [Serializable]
    public sealed class PendingEventDto
    {
        /// <summary>Occurrence id.</summary>
        public int InstanceId;

        /// <summary>Event id.</summary>
        public string EventId;

        /// <summary>EventOrigin name.</summary>
        public string Origin;

        /// <summary>Week it occurred.</summary>
        public int FiredWeek;

        /// <summary>Deadline week.</summary>
        public int DeadlineWeek;
    }
}
