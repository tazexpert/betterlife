using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Explains where a stat value comes from.
    /// </summary>
    public sealed class BreakdownState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="title">Stat name.</param>
        /// <param name="valueText">Current value text.</param>
        /// <param name="heading">Section heading (formula or recent changes).</param>
        /// <param name="lines">Lines, largest first.</param>
        /// <param name="emptyText">Text shown when there are no lines.</param>
        public BreakdownState(string title, string valueText, string heading, IReadOnlyList<BreakdownLineState> lines, string emptyText)
        {
            Title = title;
            ValueText = valueText;
            Heading = heading;
            Lines = lines;
            EmptyText = emptyText;
        }

        /// <summary>Stat name.</summary>
        public string Title { get; }

        /// <summary>Current value text.</summary>
        public string ValueText { get; }

        /// <summary>Section heading.</summary>
        public string Heading { get; }

        /// <summary>Lines, largest first.</summary>
        public IReadOnlyList<BreakdownLineState> Lines { get; }

        /// <summary>Text shown when there are no lines.</summary>
        public string EmptyText { get; }
    }
}
