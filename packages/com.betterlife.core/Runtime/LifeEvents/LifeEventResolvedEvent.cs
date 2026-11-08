using System;
using BetterLife.Core.Events;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Published when an event is answered (by the player or automatically).
    /// </summary>
    public sealed class LifeEventResolvedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="resolution">The resolution.</param>
        public LifeEventResolvedEvent(EventResolution resolution)
        {
            Resolution = resolution ?? throw new ArgumentNullException(nameof(resolution));
        }

        /// <summary>The resolution.</summary>
        public EventResolution Resolution { get; }
    }
}
