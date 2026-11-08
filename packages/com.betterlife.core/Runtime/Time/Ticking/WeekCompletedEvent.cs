using BetterLife.Core.Events;

namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// Published after every phase of a week has run.
    /// </summary>
    public sealed class WeekCompletedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="week">The completed week.</param>
        public WeekCompletedEvent(GameDate week)
        {
            Week = week;
        }

        /// <summary>The completed week.</summary>
        public GameDate Week { get; }

        /// <summary>True when the week started a month.</summary>
        public bool WasMonthStart => Week.IsMonthStart;

        /// <summary>True when the week started a year.</summary>
        public bool WasYearStart => Week.IsYearStart;
    }
}
