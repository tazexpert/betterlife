namespace BetterLife.Core.Time
{
    /// <summary>
    /// Structural constants and helpers for the stylized in-game calendar (52 weeks per year).
    /// </summary>
    public static class GameCalendar
    {
        /// <summary>The first in-game year. <see cref="GameDate.TotalWeeks"/> zero is week 0 of this year.</summary>
        public const int EpochYear = 1926;

        /// <summary>Number of weeks in one in-game year.</summary>
        public const int WeeksPerYear = 52;

        /// <summary>Number of days in one week.</summary>
        public const int DaysPerWeek = 7;

        /// <summary>Number of months in one year.</summary>
        public const int MonthsPerYear = 12;

        /// <summary>Number of days in a non-leap Gregorian year.</summary>
        public const int DaysPerCommonYear = 365;

        // Zero-based day-of-year on which each month starts in a common (non-leap) year.
        private static readonly int[] CommonYearMonthStartDays = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };

        /// <summary>
        /// Returns the month (1-12) containing the given zero-based day of a common year.
        /// </summary>
        /// <param name="dayOfYear">Zero-based day of year, 0 to 364.</param>
        /// <returns>Month number from 1 to 12.</returns>
        public static int MonthOfCommonYearDay(int dayOfYear)
        {
            if (dayOfYear < 0 || dayOfYear >= DaysPerCommonYear)
            {
                throw new System.ArgumentOutOfRangeException(nameof(dayOfYear), dayOfYear, "Day of year must be in [0, 364].");
            }

            for (int i = MonthsPerYear - 1; i > 0; i--)
            {
                if (dayOfYear >= CommonYearMonthStartDays[i])
                {
                    return i + 1;
                }
            }
            return 1;
        }
    }
}
