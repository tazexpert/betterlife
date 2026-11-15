using BetterLife.Core.Localization;
using BetterLife.Core.Settings;

namespace BetterLife.Presentation.Screens
{
    /// <summary>
    /// Applies the player's settings (implemented by the composition root, which owns the UI and the life).
    /// </summary>
    public interface ISettingsController
    {
        /// <summary>Current settings.</summary>
        GameSettings Settings { get; }

        /// <summary>Language in use.</summary>
        string Language { get; }

        /// <summary>All string tables (to list the languages).</summary>
        StringTables Tables { get; }

        /// <summary>Content version.</summary>
        int ContentVersion { get; }

        /// <summary>Undo uses left today, or null when unknown.</summary>
        int? UndoLeftToday { get; }

        /// <summary>
        /// Switches the language (the UI is rebuilt).
        /// </summary>
        /// <param name="language">Language code.</param>
        void SetLanguage(string language);

        /// <summary>
        /// Turns vice content on or off for this and future lives.
        /// </summary>
        /// <param name="enabled">Whether vice content appears.</param>
        void SetViceContent(bool enabled);

        /// <summary>
        /// Forgets every unlocked achievement.
        /// </summary>
        void ResetAchievements();
    }
}
