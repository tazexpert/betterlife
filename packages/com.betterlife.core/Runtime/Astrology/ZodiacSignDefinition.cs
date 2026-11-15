using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Astrology
{
    /// <summary>
    /// A Thai (sidereal) zodiac sign and the calendar day it starts.
    /// </summary>
    public sealed class ZodiacSignDefinition : IDefinition
    {
        /// <summary>
        /// Creates a sign.
        /// </summary>
        /// <param name="id">Sign id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="startMonth">Month the sign starts (1-12).</param>
        /// <param name="startDay">Day the sign starts (valid in a leap year).</param>
        /// <param name="modifiers">Fortune points.</param>
        public ZodiacSignDefinition(string id, string nameKey, int startMonth, int startDay, FortuneModifiers modifiers)
        {
            if (startMonth < 1 || startMonth > GameCalendar.MonthsPerYear)
            {
                throw new ArgumentOutOfRangeException(nameof(startMonth), startMonth, "Month must be in [1, 12].");
            }
            if (startDay < 1 || startDay > CalendarDate.DaysInMonth(LeapReferenceYear, startMonth))
            {
                throw new ArgumentOutOfRangeException(nameof(startDay), startDay, "Day is outside the month.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            StartMonth = startMonth;
            StartDay = startDay;
            Modifiers = modifiers ?? throw new ArgumentNullException(nameof(modifiers));
        }

        private const int LeapReferenceYear = 2000;

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Month the sign starts.</summary>
        public int StartMonth { get; }

        /// <summary>Day the sign starts.</summary>
        public int StartDay { get; }

        /// <summary>Fortune points.</summary>
        public FortuneModifiers Modifiers { get; }

        /// <summary>Start as a sortable month-day number (MMDD).</summary>
        public int StartKey => StartMonth * MonthDayFactor + StartDay;

        internal const int MonthDayFactor = 100;
    }
}
