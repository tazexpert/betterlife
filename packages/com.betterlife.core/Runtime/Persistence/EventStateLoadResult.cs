using System.Collections.Generic;
using BetterLife.Core.LifeEvents;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// A loaded event state plus warnings about entries whose event no longer exists.
    /// </summary>
    public sealed class EventStateLoadResult
    {
        /// <summary>
        /// Creates a result.
        /// </summary>
        /// <param name="state">Loaded state.</param>
        /// <param name="warnings">Warnings for logs.</param>
        public EventStateLoadResult(EventState state, IReadOnlyList<string> warnings)
        {
            State = state;
            Warnings = warnings;
        }

        /// <summary>Loaded state.</summary>
        public EventState State { get; }

        /// <summary>Warnings for logs.</summary>
        public IReadOnlyList<string> Warnings { get; }
    }
}
