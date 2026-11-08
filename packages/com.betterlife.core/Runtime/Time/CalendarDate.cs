using System;

namespace BetterLife.Core.Time
{
    /// <summary>
    /// Immutable real Gregorian date (day precision), used for birth dates and astrology.
    /// </summary>
    public readonly struct CalendarDate : IEquatable<CalendarDate>
    {
        private const int FebruaryMonth = 2;
        private const int LeapFebruaryDays = 29;
        private const int LeapCycleYears = 4;
        private const int CenturyYears = 100;
        private const int GregorianCycleYears = 400;
        private const int MaxDaysPerMonthHashSpan = 32;

        private static readonly int[] CommonMonthDays = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        // Sakamoto's day-of-week month offsets.
        private static readonly int[] DayOfWeekMonthOffsets = { 0, 3, 2, 5, 0, 3, 5, 1, 4, 6, 2, 4 };

        private readonly int _year;
        private readonly int _month;
        private readonly int _day;

        /// <summary>
        /// Creates a validated calendar date.
        /// </summary>
        /// <param name="year">Year (Common Era), must be positive.</param>
        /// <param name="month">Month, 1 to 12.</param>
        /// <param name="day">Day of month, 1 to the month's length.</param>
        public CalendarDate(int year, int month, int day)
        {
            if (year < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(year), year, "Year must be positive.");
            }
            if (month < 1 || month > GameCalendar.MonthsPerYear)
            {
                throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be in [1, 12].");
            }
            if (day < 1 || day > DaysInMonth(year, month))
            {
                throw new ArgumentOutOfRangeException(nameof(day), day, "Day is outside the month's range.");
            }

            _year = year;
            _month = month;
            _day = day;
        }

        /// <summary>Year (Common Era).</summary>
        public int Year => _year;

        /// <summary>Month, 1 to 12.</summary>
        public int Month => _month;

        /// <summary>Day of month.</summary>
        public int Day => _day;

        /// <summary>Gregorian day of week.</summary>
        public DayOfWeek DayOfWeek
        {
            get
            {
                int y = _month < 3 ? _year - 1 : _year;
                int index = (y + y / LeapCycleYears - y / CenturyYears + y / GregorianCycleYears
                             + DayOfWeekMonthOffsets[_month - 1] + _day) % GameCalendar.DaysPerWeek;
                return (DayOfWeek)index;
            }
        }

        /// <summary>Zero-based day of year (0 to 365).</summary>
        public int DayOfYear
        {
            get
            {
                int days = _day - 1;
                for (int m = 1; m < _month; m++)
                {
                    days += DaysInMonth(_year, m);
                }
                return days;
            }
        }

        /// <summary>
        /// Returns true when the given year is a Gregorian leap year.
        /// </summary>
        /// <param name="year">Year to test.</param>
        /// <returns>True for leap years.</returns>
        public static bool IsLeapYear(int year)
        {
            return (year % LeapCycleYears == 0 && year % CenturyYears != 0) || year % GregorianCycleYears == 0;
        }

        /// <summary>
        /// Returns the number of days in a month.
        /// </summary>
        /// <param name="year">Year.</param>
        /// <param name="month">Month, 1 to 12.</param>
        /// <returns>Days in that month.</returns>
        public static int DaysInMonth(int year, int month)
        {
            if (month < 1 || month > GameCalendar.MonthsPerYear)
            {
                throw new ArgumentOutOfRangeException(nameof(month), month, "Month must be in [1, 12].");
            }
            return month == FebruaryMonth && IsLeapYear(year) ? LeapFebruaryDays : CommonMonthDays[month - 1];
        }

        /// <summary>
        /// Returns the in-game week containing this date. The last 1-2 days of a year fall into week 51.
        /// </summary>
        /// <returns>The containing game week.</returns>
        public GameDate ToGameDate()
        {
            int week = Math.Min(DayOfYear / GameCalendar.DaysPerWeek, GameCalendar.WeeksPerYear - 1);
            return GameDate.FromYearWeek(_year, week);
        }

        /// <summary>
        /// Returns the age in whole years at the given game week. The birthday counts from the week containing it.
        /// </summary>
        /// <param name="now">Current game date; must not be before this date's week.</param>
        /// <returns>Age in years.</returns>
        public int AgeAt(GameDate now)
        {
            GameDate birthWeek = ToGameDate();
            if (now < birthWeek)
            {
                throw new ArgumentOutOfRangeException(nameof(now), now, "Date is before the birth week.");
            }

            int age = now.Year - birthWeek.Year;
            return now.WeekOfYear < birthWeek.WeekOfYear ? age - 1 : age;
        }

        /// <summary>Value equality.</summary>
        /// <param name="other">Date to compare.</param>
        /// <returns>True when same day.</returns>
        public bool Equals(CalendarDate other) => _year == other._year && _month == other._month && _day == other._day;

        /// <summary>Value equality.</summary>
        /// <param name="obj">Object to compare.</param>
        /// <returns>True when same day.</returns>
        public override bool Equals(object obj) => obj is CalendarDate other && Equals(other);

        /// <summary>Hash code from year, month and day.</summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode() => (_year * GameCalendar.MonthsPerYear + _month) * MaxDaysPerMonthHashSpan + _day;

        /// <summary>Debug representation in ISO format. Not for player-facing text.</summary>
        /// <returns>Debug string.</returns>
        public override string ToString() => $"{_year:0000}-{_month:00}-{_day:00}";

        /// <summary>Equality operator.</summary>
        /// <param name="left">Left date.</param>
        /// <param name="right">Right date.</param>
        /// <returns>True when equal.</returns>
        public static bool operator ==(CalendarDate left, CalendarDate right) => left.Equals(right);

        /// <summary>Inequality operator.</summary>
        /// <param name="left">Left date.</param>
        /// <param name="right">Right date.</param>
        /// <returns>True when not equal.</returns>
        public static bool operator !=(CalendarDate left, CalendarDate right) => !left.Equals(right);
    }
}
