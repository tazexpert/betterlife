using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of the event state.</summary>
    [Serializable]
    public sealed class EventStateDto
    {
        /// <summary>Per-event records.</summary>
        public List<EventRecordDto> Records = new List<EventRecordDto>();

        /// <summary>Scheduled events.</summary>
        public List<ScheduledEventDto> Scheduled = new List<ScheduledEventDto>();

        /// <summary>Inbox.</summary>
        public List<PendingEventDto> Inbox = new List<PendingEventDto>();

        /// <summary>Recent tones.</summary>
        public List<ToneEntryDto> Tones = new List<ToneEntryDto>();

        /// <summary>History.</summary>
        public List<EventHistoryDto> History = new List<EventHistoryDto>();

        /// <summary>Next occurrence id.</summary>
        public int NextInstanceId;
    }
}
