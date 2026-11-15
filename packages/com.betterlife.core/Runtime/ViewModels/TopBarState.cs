namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// What the top bar shows.
    /// </summary>
    public sealed class TopBarState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="name">Character name.</param>
        /// <param name="ageText">Age line.</param>
        /// <param name="dateText">Date line.</param>
        /// <param name="cashText">Cash (prices of the era).</param>
        /// <param name="inboxCount">Events waiting for a decision.</param>
        /// <param name="canAdvance">False once the life is over.</param>
        public TopBarState(string name, string ageText, string dateText, string cashText, int inboxCount, bool canAdvance)
        {
            Name = name;
            AgeText = ageText;
            DateText = dateText;
            CashText = cashText;
            InboxCount = inboxCount;
            CanAdvance = canAdvance;
        }

        /// <summary>Character name.</summary>
        public string Name { get; }

        /// <summary>Age line.</summary>
        public string AgeText { get; }

        /// <summary>Date line.</summary>
        public string DateText { get; }

        /// <summary>Cash (prices of the era).</summary>
        public string CashText { get; }

        /// <summary>Events waiting for a decision.</summary>
        public int InboxCount { get; }

        /// <summary>False once the life is over.</summary>
        public bool CanAdvance { get; }
    }
}
