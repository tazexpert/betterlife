using System.Collections.Generic;
using BetterLife.Core.Luck;
using BetterLife.Core.Rules;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// What the event dialog shows for one choice.
    /// </summary>
    public sealed class ChoiceView
    {
        /// <summary>
        /// Creates a view.
        /// </summary>
        /// <param name="choice">The choice.</param>
        /// <param name="isAvailable">Whether it can be picked now.</param>
        /// <param name="isVisible">Whether it is shown at all.</param>
        /// <param name="requirement">Why it is locked or unlocked, or null without a requirement.</param>
        /// <param name="chance">Success chance with breakdown, or null for a guaranteed outcome.</param>
        /// <param name="successPreview">Preview of the success outcome.</param>
        /// <param name="failurePreview">Preview of the failure outcome (empty without a chance).</param>
        public ChoiceView(EventChoice choice, bool isAvailable, bool isVisible, ConditionTrace requirement, ChanceBreakdown chance,
            IReadOnlyList<RuleText> successPreview, IReadOnlyList<RuleText> failurePreview)
        {
            Choice = choice;
            IsAvailable = isAvailable;
            IsVisible = isVisible;
            Requirement = requirement;
            Chance = chance;
            SuccessPreview = successPreview;
            FailurePreview = failurePreview;
        }

        /// <summary>The choice.</summary>
        public EventChoice Choice { get; }

        /// <summary>Whether it can be picked now.</summary>
        public bool IsAvailable { get; }

        /// <summary>Whether it is shown at all.</summary>
        public bool IsVisible { get; }

        /// <summary>Requirement trace, or null.</summary>
        public ConditionTrace Requirement { get; }

        /// <summary>Success chance, or null.</summary>
        public ChanceBreakdown Chance { get; }

        /// <summary>Preview of the success outcome.</summary>
        public IReadOnlyList<RuleText> SuccessPreview { get; }

        /// <summary>Preview of the failure outcome.</summary>
        public IReadOnlyList<RuleText> FailurePreview { get; }
    }
}
