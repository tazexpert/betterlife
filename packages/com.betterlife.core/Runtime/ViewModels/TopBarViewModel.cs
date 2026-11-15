using System;
using System.Globalization;
using BetterLife.Core.Localization;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the top bar text from a running life.
    /// </summary>
    public static class TopBarViewModel
    {
        /// <summary>Key of the name pattern.</summary>
        public const string NameKey = "topbar.name";
        /// <summary>Key of the age pattern.</summary>
        public const string AgeKey = "topbar.age";
        /// <summary>Key of the date pattern.</summary>
        public const string DateKey = "topbar.date";
        /// <summary>Prefix of the month name keys (month.m1 ... month.m12).</summary>
        public const string MonthKeyPrefix = "month.m";

        /// <summary>
        /// Builds the state.
        /// </summary>
        /// <param name="session">Running life.</param>
        /// <param name="localizer">Localizer.</param>
        /// <param name="money">Money formatter.</param>
        /// <returns>The state.</returns>
        public static TopBarState Build(LifeSession session, ILocalizer localizer, MoneyFormatter money)
        {
            if (session == null || localizer == null || money == null)
            {
                throw new ArgumentNullException(nameof(session));
            }
            GameDate now = session.TimeState.Clock.Now;
            var character = session.Character;
            string name = localizer.Format(NameKey, ("given", character.Name.GivenName), ("family", character.Name.FamilyName));
            string age = localizer.Format(AgeKey, ("age", character.AgeAt(now).ToString(CultureInfo.InvariantCulture)));
            string date = localizer.Format(DateKey, ("month", localizer.Get(MonthKeyPrefix + now.Month.ToString(CultureInfo.InvariantCulture))),
                ("year", now.Year.ToString(CultureInfo.InvariantCulture)));
            string cash = money.Compact(session.Economy.State.Nominal(character.Wallet.Cash));
            return new TopBarState(name, age, date, cash, session.Events.Inbox.Count, !session.IsOver);
        }
    }
}
