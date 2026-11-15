using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Outcome of a player action outside events (an interaction, a scam report...).
    /// </summary>
    public sealed class ActionResultState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="title">Headline.</param>
        /// <param name="succeeded">Whether it went well.</param>
        /// <param name="lines">Details.</param>
        public ActionResultState(string title, bool succeeded, IReadOnlyList<string> lines)
        {
            Title = title;
            Succeeded = succeeded;
            Lines = lines;
        }

        /// <summary>Headline.</summary>
        public string Title { get; }

        /// <summary>Whether it went well.</summary>
        public bool Succeeded { get; }

        /// <summary>Details.</summary>
        public IReadOnlyList<string> Lines { get; }
    }
}
