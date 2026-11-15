using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// The end-of-life summary: eulogy, key numbers, achievements of this life and who can carry on.
    /// </summary>
    public sealed class LifeSummaryState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="title">Name.</param>
        /// <param name="subtitle">Years and age.</param>
        /// <param name="eulogy">Eulogy sentences.</param>
        /// <param name="facts">Key numbers.</param>
        /// <param name="achievements">Achievements unlocked during this life.</param>
        /// <param name="heirs">Children who can carry on (empty: start a new family).</param>
        /// <param name="shareText">Plain text to copy and share.</param>
        public LifeSummaryState(string title, string subtitle, IReadOnlyList<string> eulogy, IReadOnlyList<ValueRow> facts, IReadOnlyList<string> achievements,
            IReadOnlyList<HeirOption> heirs, string shareText)
        {
            Title = title;
            Subtitle = subtitle;
            Eulogy = eulogy;
            Facts = facts;
            Achievements = achievements;
            Heirs = heirs;
            ShareText = shareText;
        }

        /// <summary>Name.</summary>
        public string Title { get; }

        /// <summary>Years and age.</summary>
        public string Subtitle { get; }

        /// <summary>Eulogy sentences.</summary>
        public IReadOnlyList<string> Eulogy { get; }

        /// <summary>Key numbers.</summary>
        public IReadOnlyList<ValueRow> Facts { get; }

        /// <summary>Achievements unlocked during this life.</summary>
        public IReadOnlyList<string> Achievements { get; }

        /// <summary>Children who can carry on.</summary>
        public IReadOnlyList<HeirOption> Heirs { get; }

        /// <summary>Plain text to copy and share.</summary>
        public string ShareText { get; }
    }
}
