using System.Collections.Generic;
using BetterLife.Core.Time.Budget;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// A loaded time state plus warnings about allocations whose activity no longer exists.
    /// </summary>
    public sealed class TimeStateLoadResult
    {
        /// <summary>
        /// Creates a result.
        /// </summary>
        /// <param name="state">Loaded state.</param>
        /// <param name="warnings">Warnings for logs.</param>
        public TimeStateLoadResult(TimeState state, IReadOnlyList<string> warnings)
        {
            State = state;
            Warnings = warnings;
        }

        /// <summary>Loaded state.</summary>
        public TimeState State { get; }

        /// <summary>Warnings for logs.</summary>
        public IReadOnlyList<string> Warnings { get; }
    }
}
