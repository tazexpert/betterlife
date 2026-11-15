using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// The Life tab: the weekly plan and the habits that can be quit.
    /// </summary>
    public sealed class LifePlanState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="summary">Hours left for the default activity.</param>
        /// <param name="activities">Activities in catalog order (unavailable ones left out).</param>
        /// <param name="issues">Problems with the plan.</param>
        /// <param name="habits">Habits with their quit methods.</param>
        public LifePlanState(string summary, IReadOnlyList<ActivityRow> activities, IReadOnlyList<string> issues, IReadOnlyList<OfferGroup> habits)
        {
            Summary = summary;
            Activities = activities;
            Issues = issues;
            Habits = habits;
        }

        /// <summary>Hours left for the default activity.</summary>
        public string Summary { get; }

        /// <summary>Activities.</summary>
        public IReadOnlyList<ActivityRow> Activities { get; }

        /// <summary>Problems with the plan.</summary>
        public IReadOnlyList<string> Issues { get; }

        /// <summary>Habits with their quit methods.</summary>
        public IReadOnlyList<OfferGroup> Habits { get; }
    }
}
