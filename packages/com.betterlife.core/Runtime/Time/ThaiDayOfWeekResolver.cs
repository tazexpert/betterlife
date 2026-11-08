using System;

namespace BetterLife.Core.Time
{
    /// <summary>
    /// Resolves the Thai astrological birth day. The Thai astrological day starts at sunrise,
    /// so births before sunrise belong to the previous day, and Wednesday evening is Rahu (Wednesday night).
    /// </summary>
    public static class ThaiDayOfWeekResolver
    {
        /// <summary>Hour at which the astrological day begins.</summary>
        public const int SunriseHour = 6;

        /// <summary>Hour at which Wednesday night (Rahu) begins.</summary>
        public const int WednesdayNightStartHour = 18;

        /// <summary>Number of hours in a day.</summary>
        public const int HoursPerDay = 24;

        /// <summary>
        /// Resolves the astrological day for a birth on <paramref name="date"/> at <paramref name="hour"/>.
        /// </summary>
        /// <param name="date">Civil birth date.</param>
        /// <param name="hour">Civil birth hour, 0 to 23.</param>
        /// <returns>The Thai astrological day of week.</returns>
        public static ThaiDayOfWeek Resolve(CalendarDate date, int hour)
        {
            if (hour < 0 || hour >= HoursPerDay)
            {
                throw new ArgumentOutOfRangeException(nameof(hour), hour, "Hour must be in [0, 23].");
            }

            bool beforeSunrise = hour < SunriseHour;
            int civilDay = (int)date.DayOfWeek;
            int astroDay = beforeSunrise
                ? (civilDay + GameCalendar.DaysPerWeek - 1) % GameCalendar.DaysPerWeek
                : civilDay;

            bool isNight = beforeSunrise || hour >= WednesdayNightStartHour;
            if (astroDay == (int)DayOfWeek.Wednesday && isNight)
            {
                return ThaiDayOfWeek.WednesdayNight;
            }
            return (ThaiDayOfWeek)astroDay;
        }
    }
}
