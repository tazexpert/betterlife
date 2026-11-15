using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Birth chart, this year's fortune and karma.
    /// </summary>
    public sealed class AstrologyState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="profile">Zodiac, birth day, year animal, element.</param>
        /// <param name="yearTitle">Heading of this year's fortune.</param>
        /// <param name="fortunes">Fortune per area.</param>
        /// <param name="karma">Merit, sin, chance points, pending karmic debt.</param>
        public AstrologyState(IReadOnlyList<ValueRow> profile, string yearTitle, IReadOnlyList<FortuneRow> fortunes, IReadOnlyList<ValueRow> karma)
        {
            Profile = profile;
            YearTitle = yearTitle;
            Fortunes = fortunes;
            Karma = karma;
        }

        /// <summary>Zodiac, birth day, year animal, element.</summary>
        public IReadOnlyList<ValueRow> Profile { get; }

        /// <summary>Heading of this year's fortune.</summary>
        public string YearTitle { get; }

        /// <summary>Fortune per area.</summary>
        public IReadOnlyList<FortuneRow> Fortunes { get; }

        /// <summary>Merit, sin, chance points, pending karmic debt.</summary>
        public IReadOnlyList<ValueRow> Karma { get; }
    }
}
