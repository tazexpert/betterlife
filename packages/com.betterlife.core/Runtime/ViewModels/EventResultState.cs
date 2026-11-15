using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// What happened after answering an event.
    /// </summary>
    public sealed class EventResultState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="title">Event title.</param>
        /// <param name="resultText">Outcome story text.</param>
        /// <param name="succeeded">Whether the choice succeeded (always true without a roll).</param>
        /// <param name="rollText">Roll summary, or null when there was no roll.</param>
        /// <param name="lines">Applied effects.</param>
        public EventResultState(string title, string resultText, bool succeeded, string rollText, IReadOnlyList<string> lines)
        {
            Title = title;
            ResultText = resultText;
            Succeeded = succeeded;
            RollText = rollText;
            Lines = lines;
        }

        /// <summary>Event title.</summary>
        public string Title { get; }

        /// <summary>Outcome story text.</summary>
        public string ResultText { get; }

        /// <summary>Whether the choice succeeded.</summary>
        public bool Succeeded { get; }

        /// <summary>Roll summary, or null.</summary>
        public string RollText { get; }

        /// <summary>Applied effects.</summary>
        public IReadOnlyList<string> Lines { get; }
    }
}
