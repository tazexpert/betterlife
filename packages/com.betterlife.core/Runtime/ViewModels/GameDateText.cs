using System.Globalization;
using BetterLife.Core.Localization;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Formats game dates for display ("March 2031").
    /// </summary>
    public static class GameDateText
    {
        /// <summary>Month and year text key (placeholders <c>{month}</c>, <c>{year}</c>).</summary>
        public const string DateKey = "topbar.date";
        /// <summary>Month name key prefix (<c>month.m1</c> ... <c>month.m12</c>).</summary>
        public const string MonthKeyPrefix = "month.m";

        /// <summary>
        /// Formats a date as month and year.
        /// </summary>
        /// <param name="date">Date.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The text.</returns>
        public static string MonthYear(GameDate date, ILocalizer localizer)
        {
            return localizer.Format(DateKey,
                ("month", localizer.Get(MonthKeyPrefix + date.Month.ToString(CultureInfo.InvariantCulture))),
                ("year", date.Year.ToString(CultureInfo.InvariantCulture)));
        }
    }
}
