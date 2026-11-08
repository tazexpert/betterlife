using System.Collections.Generic;
using BetterLife.Core.Domain.Characters;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// A loaded character plus warnings about saved data that no longer matches the content (skipped entries).
    /// </summary>
    public sealed class CharacterLoadResult
    {
        /// <summary>
        /// Creates a result.
        /// </summary>
        /// <param name="character">The loaded character.</param>
        /// <param name="warnings">Human-readable warnings for logs (not shown to players).</param>
        public CharacterLoadResult(Character character, IReadOnlyList<string> warnings)
        {
            Character = character;
            Warnings = warnings;
        }

        /// <summary>The loaded character.</summary>
        public Character Character { get; }

        /// <summary>Warnings about skipped entries.</summary>
        public IReadOnlyList<string> Warnings { get; }
    }
}
