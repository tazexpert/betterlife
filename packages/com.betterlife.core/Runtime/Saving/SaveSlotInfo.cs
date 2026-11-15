using BetterLife.Core.Persistence;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// One save slot as the slot list shows it.
    /// </summary>
    public sealed class SaveSlotInfo
    {
        /// <summary>
        /// Creates the info.
        /// </summary>
        /// <param name="slot">Slot name.</param>
        /// <param name="summary">Summary, or null when unreadable.</param>
        /// <param name="error">Why it is unreadable, or null.</param>
        public SaveSlotInfo(string slot, SaveSummaryDto summary, SaveErrorKind? error)
        {
            Slot = slot;
            Summary = summary;
            Error = error;
        }

        /// <summary>Slot name.</summary>
        public string Slot { get; }

        /// <summary>Summary, or null when unreadable.</summary>
        public SaveSummaryDto Summary { get; }

        /// <summary>Why it is unreadable, or null.</summary>
        public SaveErrorKind? Error { get; }

        /// <summary>True when the slot can be loaded.</summary>
        public bool Readable => Error == null;
    }
}
