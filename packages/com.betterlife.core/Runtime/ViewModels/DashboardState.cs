using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Everything the Home dashboard shows.
    /// </summary>
    public sealed class DashboardState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="cards">Core stat cards, in catalog order.</param>
        /// <param name="habits">Visible habit cards (empty hides the section).</param>
        /// <param name="pendingDecisions">Number of events waiting for an answer.</param>
        /// <param name="inboxText">Inbox shortcut text.</param>
        /// <param name="family">Relationship status and children.</param>
        /// <param name="avatarPath">Resources path of the player's picture, or null.</param>
        public DashboardState(IReadOnlyList<StatCardState> cards, IReadOnlyList<HabitCardState> habits, int pendingDecisions, string inboxText,
            IReadOnlyList<ValueRow> family, string avatarPath = null)
        {
            AvatarPath = avatarPath;
            Family = family;
            Cards = cards;
            Habits = habits;
            PendingDecisions = pendingDecisions;
            InboxText = inboxText;
        }

        /// <summary>Core stat cards.</summary>
        public IReadOnlyList<StatCardState> Cards { get; }

        /// <summary>Visible habit cards.</summary>
        public IReadOnlyList<HabitCardState> Habits { get; }

        /// <summary>Number of events waiting for an answer.</summary>
        public int PendingDecisions { get; }

        /// <summary>Inbox shortcut text.</summary>
        public string InboxText { get; }

        /// <summary>Relationship status and children.</summary>
        public IReadOnlyList<ValueRow> Family { get; }

        /// <summary>Resources path of the player's picture, or null.</summary>
        public string AvatarPath { get; }
    }
}
