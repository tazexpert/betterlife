using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One moment of the character's life.
    /// </summary>
    public sealed class TimelineEntry
    {
        /// <summary>
        /// Creates an entry.
        /// </summary>
        /// <param name="week">When it happened.</param>
        /// <param name="kind">What it is about.</param>
        /// <param name="dateText">Month and year.</param>
        /// <param name="ageText">The character's age then.</param>
        /// <param name="title">What happened.</param>
        /// <param name="detail">More detail, or empty.</param>
        public TimelineEntry(GameDate week, TimelineEntryKind kind, string dateText, string ageText, string title, string detail)
        {
            Week = week;
            Kind = kind;
            DateText = dateText;
            AgeText = ageText;
            Title = title;
            Detail = detail ?? string.Empty;
        }

        /// <summary>When it happened.</summary>
        public GameDate Week { get; }

        /// <summary>What it is about.</summary>
        public TimelineEntryKind Kind { get; }

        /// <summary>Month and year.</summary>
        public string DateText { get; }

        /// <summary>The character's age then.</summary>
        public string AgeText { get; }

        /// <summary>What happened.</summary>
        public string Title { get; }

        /// <summary>More detail, or empty.</summary>
        public string Detail { get; }
    }
}
