using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Message from the "event.schedule" effect asking the event system to schedule an event.
    /// </summary>
    public sealed class ScheduleEventRequest : IGameEvent
    {
        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="eventId">Event to schedule.</param>
        /// <param name="dueOn">Week it should occur.</param>
        public ScheduleEventRequest(string eventId, GameDate dueOn)
        {
            EventId = ContentId.Require(eventId, nameof(eventId));
            DueOn = dueOn;
        }

        /// <summary>Event to schedule.</summary>
        public string EventId { get; }

        /// <summary>Week it should occur.</summary>
        public GameDate DueOn { get; }
    }
}
