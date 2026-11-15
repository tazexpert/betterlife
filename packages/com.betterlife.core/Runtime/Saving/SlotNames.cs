using System;
using System.Text.RegularExpressions;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// Save slot names: lowercase letters, digits, '_' and '-', 1-32 characters (safe as file names).
    /// </summary>
    public static class SlotNames
    {
        /// <summary>Slot used for automatic saves.</summary>
        public const string Autosave = "autosave";

        /// <summary>Slot holding the player's Undo quota.</summary>
        public const string UndoQuota = "profile_undo";
        /// <summary>Achievements unlocked across all lives.</summary>
        public const string Achievements = "profile_achievements";
        /// <summary>Player settings (language, content).</summary>
        public const string Settings = "profile_settings";
        /// <summary>Prefix of player profile slots (not lives).</summary>
        public const string ProfilePrefix = "profile_";

        private const int MaxLength = 32;
        private static readonly Regex Pattern = new Regex("^[a-z0-9_-]{1," + MaxLength + "}$", RegexOptions.CultureInvariant);

        /// <summary>
        /// Returns true for a valid slot name.
        /// </summary>
        /// <param name="slot">Slot name.</param>
        /// <returns>True when valid.</returns>
        public static bool IsValid(string slot) => slot != null && Pattern.IsMatch(slot);

        /// <summary>
        /// Whether a slot holds the player profile rather than a life.
        /// </summary>
        /// <param name="slot">Slot name.</param>
        /// <returns>True for profile slots.</returns>
        public static bool IsProfile(string slot) => slot != null && slot.StartsWith(ProfilePrefix, System.StringComparison.Ordinal);

        /// <summary>
        /// Throws for an invalid slot name.
        /// </summary>
        /// <param name="slot">Slot name.</param>
        /// <returns>The slot name.</returns>
        public static string Require(string slot)
        {
            if (!IsValid(slot))
            {
                throw new ArgumentException($"Invalid save slot '{slot}'.", nameof(slot));
            }
            return slot;
        }
    }
}
