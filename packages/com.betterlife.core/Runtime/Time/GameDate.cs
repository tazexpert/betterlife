using System;

namespace BetterLife.Core.Time
{
    /// <summary>
    /// Immutable in-game date measured in whole weeks since <see cref="GameCalendar.EpochYear"/>.
    /// Months are derived from the day of year, so every year has exactly 12 month starts.
    /// </summary>
    public readonly struct GameDate : IEquatable<GameDate>, IComparable<GameDate>
    {
        private readonly int _totalWeeks;

        /// <summary>
        /// Creates a date from the number of weeks since the epoch. Negative values are dates before the epoch.
        /// </summary>
        /// <param name="totalWeeks">Weeks since week 0 of <see cref="GameCalendar.EpochYear"/>.</param>
        public GameDate(int totalWeeks)
        {
            _totalWeeks = totalWeeks;
        }

        /// <summary>Weeks since week 0 of <see cref="GameCalendar.EpochYear"/>. This is the only stored value.</summary>
        public int TotalWeeks => _totalWeeks;

        /// <summary>The calendar year (Common Era).</summary>
        public int Year => GameCalendar.EpochYear + FloorDiv(_totalWeeks, GameCalendar.WeeksPerYear);

        /// <summary>Zero-based week within the year, 0 to 51.</summary>
        public int WeekOfYear => FloorMod(_totalWeeks, GameCalendar.WeeksPerYear);

        /// <summary>Zero-based day of year on which this week starts.</summary>
        public int DayOfYear => WeekOfYear * GameCalendar.DaysPerWeek;

        /// <summary>Month number from 1 to 12, derived from the day on which this week starts.</summary>
        public int Month => GameCalendar.MonthOfCommonYearDay(DayOfYear);

        /// <summary>True when this is the first week of a month (monthly finance tick).</summary>
        public bool IsMonthStart => Month != AddWeeks(-1).Month;

        /// <summary>True when this is the first week of a year (yearly macro tick).</summary>
        public bool IsYearStart => WeekOfYear == 0;

        /// <summary>
        /// Creates a date from a year and a zero-based week of that year.
        /// </summary>
        /// <param name="year">Calendar year (Common Era).</param>
        /// <param name="weekOfYear">Zero-based week, 0 to 51.</param>
        /// <returns>The matching date.</returns>
        public static GameDate FromYearWeek(int year, int weekOfYear)
        {
            if (weekOfYear < 0 || weekOfYear >= GameCalendar.WeeksPerYear)
            {
                throw new ArgumentOutOfRangeException(nameof(weekOfYear), weekOfYear, "Week of year must be in [0, 51].");
            }

            return new GameDate(checked((year - GameCalendar.EpochYear) * GameCalendar.WeeksPerYear + weekOfYear));
        }

        /// <summary>
        /// Returns a new date shifted by the given number of weeks.
        /// </summary>
        /// <param name="weeks">Weeks to add; may be negative.</param>
        /// <returns>The shifted date.</returns>
        public GameDate AddWeeks(int weeks)
        {
            return new GameDate(checked(_totalWeeks + weeks));
        }

        /// <summary>
        /// Returns the number of weeks from this date to <paramref name="other"/> (negative if other is earlier).
        /// </summary>
        /// <param name="other">The target date.</param>
        /// <returns>Signed week difference.</returns>
        public int WeeksUntil(GameDate other)
        {
            return checked(other._totalWeeks - _totalWeeks);
        }

        /// <summary>Value equality on <see cref="TotalWeeks"/>.</summary>
        /// <param name="other">Date to compare.</param>
        /// <returns>True when both dates are the same week.</returns>
        public bool Equals(GameDate other) => _totalWeeks == other._totalWeeks;

        /// <summary>Value equality on <see cref="TotalWeeks"/>.</summary>
        /// <param name="obj">Object to compare.</param>
        /// <returns>True when obj is the same week.</returns>
        public override bool Equals(object obj) => obj is GameDate other && Equals(other);

        /// <summary>Hash based on <see cref="TotalWeeks"/>.</summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode() => _totalWeeks;

        /// <summary>Chronological ordering.</summary>
        /// <param name="other">Date to compare.</param>
        /// <returns>Negative, zero or positive.</returns>
        public int CompareTo(GameDate other) => _totalWeeks.CompareTo(other._totalWeeks);

        /// <summary>Debug representation such as "1926-W00". Not for player-facing text.</summary>
        /// <returns>Debug string.</returns>
        public override string ToString() => $"{Year}-W{WeekOfYear:00}";

        /// <summary>Equality operator.</summary>
        /// <param name="left">Left date.</param>
        /// <param name="right">Right date.</param>
        /// <returns>True when equal.</returns>
        public static bool operator ==(GameDate left, GameDate right) => left.Equals(right);

        /// <summary>Inequality operator.</summary>
        /// <param name="left">Left date.</param>
        /// <param name="right">Right date.</param>
        /// <returns>True when not equal.</returns>
        public static bool operator !=(GameDate left, GameDate right) => !left.Equals(right);

        /// <summary>Earlier-than operator.</summary>
        /// <param name="left">Left date.</param>
        /// <param name="right">Right date.</param>
        /// <returns>True when left is earlier.</returns>
        public static bool operator <(GameDate left, GameDate right) => left._totalWeeks < right._totalWeeks;

        /// <summary>Later-than operator.</summary>
        /// <param name="left">Left date.</param>
        /// <param name="right">Right date.</param>
        /// <returns>True when left is later.</returns>
        public static bool operator >(GameDate left, GameDate right) => left._totalWeeks > right._totalWeeks;

        /// <summary>Earlier-or-equal operator.</summary>
        /// <param name="left">Left date.</param>
        /// <param name="right">Right date.</param>
        /// <returns>True when left is earlier or equal.</returns>
        public static bool operator <=(GameDate left, GameDate right) => left._totalWeeks <= right._totalWeeks;

        /// <summary>Later-or-equal operator.</summary>
        /// <param name="left">Left date.</param>
        /// <param name="right">Right date.</param>
        /// <returns>True when left is later or equal.</returns>
        public static bool operator >=(GameDate left, GameDate right) => left._totalWeeks >= right._totalWeeks;

        private static int FloorDiv(int value, int divisor)
        {
            int quotient = value / divisor;
            return (value % divisor != 0 && value < 0) ? quotient - 1 : quotient;
        }

        private static int FloorMod(int value, int divisor)
        {
            int remainder = value % divisor;
            return remainder < 0 ? remainder + divisor : remainder;
        }
    }
}
