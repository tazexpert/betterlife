using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// The inbox: decisions first (soonest deadline first), then news (newest first).
    /// </summary>
    public sealed class InboxState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="decisions">Events waiting for an answer.</param>
        /// <param name="notices">Recent news.</param>
        /// <param name="emptyText">Text shown when both lists are empty.</param>
        public InboxState(IReadOnlyList<InboxItemState> decisions, IReadOnlyList<InboxItemState> notices, string emptyText)
        {
            Decisions = decisions;
            Notices = notices;
            EmptyText = emptyText;
        }

        /// <summary>Events waiting for an answer.</summary>
        public IReadOnlyList<InboxItemState> Decisions { get; }

        /// <summary>Recent news.</summary>
        public IReadOnlyList<InboxItemState> Notices { get; }

        /// <summary>Text shown when both lists are empty.</summary>
        public string EmptyText { get; }

        /// <summary>Whether there is nothing to show.</summary>
        public bool IsEmpty => Decisions.Count == 0 && Notices.Count == 0;
    }
}
