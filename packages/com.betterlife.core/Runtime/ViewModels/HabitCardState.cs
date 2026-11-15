namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// A habit card (a vice the character is getting hooked on, or quitting).
    /// </summary>
    public sealed class HabitCardState
    {
        /// <summary>
        /// Creates a card.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <param name="name">Vice name.</param>
        /// <param name="addiction">Addiction 0..100.</param>
        /// <param name="statusText">Status line (addiction level, or quitting progress).</param>
        /// <param name="isQuitting">Whether a quit attempt is running.</param>
        public HabitCardState(string viceId, string name, float addiction, string statusText, bool isQuitting)
        {
            ViceId = viceId;
            Name = name;
            Addiction = addiction;
            StatusText = statusText;
            IsQuitting = isQuitting;
        }

        /// <summary>Vice id.</summary>
        public string ViceId { get; }

        /// <summary>Vice name.</summary>
        public string Name { get; }

        /// <summary>Addiction 0..100.</summary>
        public float Addiction { get; }

        /// <summary>Status line.</summary>
        public string StatusText { get; }

        /// <summary>Whether a quit attempt is running.</summary>
        public bool IsQuitting { get; }
    }
}
