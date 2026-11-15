using System;
using System.Globalization;
using BetterLife.Core.Localization;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Small formatting helpers shared by the screens of a running life.
    /// </summary>
    public static class SessionText
    {
        /// <summary>Age text key (placeholder <c>{age}</c>).</summary>
        public const string AgeKey = "topbar.age";
        /// <summary>Format of percentages.</summary>
        public const string PercentFormat = "0.#";
        /// <summary>Percent text key (placeholder <c>{value}</c>).</summary>
        public const string PercentKey = "common.percent";

        /// <summary>
        /// Formats real (base-year) money at today's prices.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="money">Money formatter.</param>
        /// <param name="real">Real amount.</param>
        /// <returns>The text.</returns>
        public static string Money(LifeSession session, MoneyFormatter money, decimal real)
        {
            if (session == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            return money.Full(session.Economy.State.Nominal(real));
        }

        /// <summary>
        /// Formats the character's age at a week.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="week">Week.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The text.</returns>
        public static string Age(LifeSession session, GameDate week, ILocalizer localizer)
        {
            return localizer.Format(AgeKey, ("age", session.Character.AgeAt(week).ToString(CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Formats a fraction (0.05) or a percent (5) as "5%".
        /// </summary>
        /// <param name="percent">Value in percent.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The text.</returns>
        public static string Percent(float percent, ILocalizer localizer)
        {
            return localizer.Format(PercentKey, ("value", percent.ToString(PercentFormat, CultureInfo.InvariantCulture)));
        }

        /// <summary>
        /// Builds a localization key from a prefix and an enum value ("finance.category." + Income → "finance.category.income").
        /// </summary>
        /// <param name="prefix">Key prefix ending with a dot.</param>
        /// <param name="value">Enum value.</param>
        /// <returns>The key.</returns>
        public static string EnumKey(string prefix, Enum value) => prefix + value.ToString().ToLowerInvariant();
    }
}
