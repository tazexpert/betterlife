using System;
using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Characters
{
    /// <summary>
    /// Immutable birth facts: date, hour (for Thai astrology) and country.
    /// </summary>
    public sealed class BirthInfo
    {
        /// <summary>
        /// Creates birth info.
        /// </summary>
        /// <param name="date">Birth date.</param>
        /// <param name="hour">Birth hour, 0-23.</param>
        /// <param name="countryId">Country content id.</param>
        public BirthInfo(CalendarDate date, int hour, string countryId)
        {
            if (hour < 0 || hour >= ThaiDayOfWeekResolver.HoursPerDay)
            {
                throw new ArgumentOutOfRangeException(nameof(hour), hour, "Hour must be in [0, 23].");
            }

            Date = date;
            Hour = hour;
            CountryId = ContentId.Require(countryId, nameof(countryId));
        }

        /// <summary>Birth date.</summary>
        public CalendarDate Date { get; }

        /// <summary>Birth hour, 0-23.</summary>
        public int Hour { get; }

        /// <summary>Country content id.</summary>
        public string CountryId { get; }

        /// <summary>Thai astrological birth day, including Wednesday night.</summary>
        public ThaiDayOfWeek ThaiDay => ThaiDayOfWeekResolver.Resolve(Date, Hour);

        /// <summary>The game week of the birth.</summary>
        public GameDate Week => Date.ToGameDate();
    }
}
