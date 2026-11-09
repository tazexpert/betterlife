using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Sessions
{
    /// <summary>
    /// A loaded life and what had to be dropped because content changed.
    /// </summary>
    public sealed class LifeSessionLoadResult
    {
        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="session">The loaded session.</param>
        /// <param name="warnings">Warnings.</param>
        public LifeSessionLoadResult(LifeSession session, IEnumerable<string> warnings)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Warnings = (warnings ?? Enumerable.Empty<string>()).ToList();
        }

        /// <summary>The loaded session.</summary>
        public LifeSession Session { get; }

        /// <summary>Warnings (dropped content).</summary>
        public IReadOnlyList<string> Warnings { get; }
    }
}
