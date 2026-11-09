using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// Whether the last decision can be undone now.
    /// </summary>
    public sealed class UndoQuote
    {
        /// <summary>Ironman lives cannot undo.</summary>
        public const string ReasonIronman = "undo.reason.ironman";
        /// <summary>No decision to undo.</summary>
        public const string ReasonNoSnapshot = "undo.reason.no_snapshot";
        /// <summary>Today's undos are used up.</summary>
        public const string ReasonDailyLimit = "undo.reason.daily_limit";
        /// <summary>Something that may never be undone (a bet) happened since.</summary>
        public const string ReasonIrreversible = "undo.reason.irreversible";

        /// <summary>
        /// Creates the quote.
        /// </summary>
        /// <param name="reasonKeys">Why not; empty when available.</param>
        /// <param name="usesLeftToday">Undos left today.</param>
        public UndoQuote(IReadOnlyList<string> reasonKeys, int usesLeftToday)
        {
            ReasonKeys = (reasonKeys ?? throw new ArgumentNullException(nameof(reasonKeys))).ToList();
            UsesLeftToday = usesLeftToday;
        }

        /// <summary>True when Undo is possible.</summary>
        public bool Available => ReasonKeys.Count == 0;

        /// <summary>Why not.</summary>
        public IReadOnlyList<string> ReasonKeys { get; }

        /// <summary>Undos left today.</summary>
        public int UsesLeftToday { get; }
    }
}
