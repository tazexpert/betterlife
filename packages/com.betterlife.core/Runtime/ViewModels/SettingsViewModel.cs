using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Localization;
using BetterLife.Core.Settings;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Builds the settings screen.
    /// </summary>
    public static class SettingsViewModel
    {
        /// <summary>Key of a table's own language name ("English", "ไทย").</summary>
        public const string LanguageSelfKey = "language.self";
        /// <summary>Note under the current language.</summary>
        public const string CurrentKey = "settings.language.current";
        /// <summary>Turn vice content off.</summary>
        public const string ViceOffKey = "settings.vice.turn_off";
        /// <summary>Turn vice content on.</summary>
        public const string ViceOnKey = "settings.vice.turn_on";
        /// <summary>Undo left label.</summary>
        public const string UndoKey = "settings.undo_left";
        /// <summary>Content version label.</summary>
        public const string VersionKey = "settings.content_version";
        /// <summary>Achievements label.</summary>
        public const string AchievementsKey = "settings.achievements";

        /// <summary>
        /// Builds the screen.
        /// </summary>
        /// <param name="settings">Current settings.</param>
        /// <param name="language">Language in use.</param>
        /// <param name="tables">All string tables.</param>
        /// <param name="undoLeftToday">Undo uses left today, or null when unknown.</param>
        /// <param name="achievementsText">Achievement progress text.</param>
        /// <param name="contentVersion">Content version.</param>
        /// <param name="localizer">Localizer.</param>
        /// <returns>The state.</returns>
        public static SettingsState Build(GameSettings settings, string language, StringTables tables, int? undoLeftToday, string achievementsText,
            int contentVersion, ILocalizer localizer)
        {
            if (settings == null || tables == null || localizer == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            List<AmountChoice> languages = tables.Languages.Select(code =>
            {
                bool current = code == language;
                string name = tables.Find(code).Find(LanguageSelfKey) ?? code;
                return new AmountChoice(code, name, 0m, !current, current ? localizer.Get(CurrentKey) : string.Empty);
            }).ToList();
            var info = new List<ValueRow>
            {
                new ValueRow(localizer.Get(UndoKey), undoLeftToday.HasValue ? undoLeftToday.Value.ToString(CultureInfo.InvariantCulture) : "-"),
                new ValueRow(localizer.Get(AchievementsKey), achievementsText ?? string.Empty),
                new ValueRow(localizer.Get(VersionKey), contentVersion.ToString(CultureInfo.InvariantCulture)),
            };
            return new SettingsState(languages, settings.ViceContent, localizer.Get(settings.ViceContent ? ViceOffKey : ViceOnKey), info);
        }
    }
}
