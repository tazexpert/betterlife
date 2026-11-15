using System;
using BetterLife.Core.Events;

namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// Published when a time skip ends (completed or interrupted).
    /// </summary>
    public sealed class TimeAdvancedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="result">The skip outcome.</param>
        public TimeAdvancedEvent(AdvanceResult result)
        {
            Result = result ?? throw new ArgumentNullException(nameof(result));
        }

        /// <summary>The skip outcome.</summary>
        public AdvanceResult Result { get; }
    }
}
