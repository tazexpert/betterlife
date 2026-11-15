using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One visible choice of an event dialog.
    /// </summary>
    public sealed class ChoiceState
    {
        /// <summary>
        /// Creates a choice.
        /// </summary>
        /// <param name="id">Choice id.</param>
        /// <param name="text">Choice text.</param>
        /// <param name="isAvailable">Whether it can be picked.</param>
        /// <param name="chanceText">Success chance text, or null for a sure outcome.</param>
        /// <param name="successPreview">Expected effects on success.</param>
        /// <param name="failurePreview">Expected effects on failure (empty for a sure outcome).</param>
        /// <param name="lockedText">Why it is locked, or null.</param>
        public ChoiceState(string id, string text, bool isAvailable, string chanceText, IReadOnlyList<string> successPreview,
            IReadOnlyList<string> failurePreview, string lockedText)
        {
            Id = id;
            Text = text;
            IsAvailable = isAvailable;
            ChanceText = chanceText;
            SuccessPreview = successPreview;
            FailurePreview = failurePreview;
            LockedText = lockedText;
        }

        /// <summary>Choice id.</summary>
        public string Id { get; }

        /// <summary>Choice text.</summary>
        public string Text { get; }

        /// <summary>Whether it can be picked.</summary>
        public bool IsAvailable { get; }

        /// <summary>Success chance text, or null for a sure outcome.</summary>
        public string ChanceText { get; }

        /// <summary>Expected effects on success.</summary>
        public IReadOnlyList<string> SuccessPreview { get; }

        /// <summary>Expected effects on failure.</summary>
        public IReadOnlyList<string> FailurePreview { get; }

        /// <summary>Why it is locked, or null.</summary>
        public string LockedText { get; }
    }
}
