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

        private const int MaxLength = 32;
        private static readonly Regex Pattern = new Regex("^[a-z0-9_-]{1," + MaxLength + "}$", RegexOptions.CultureInvariant);

        /// <summary>
        /// Returns true for a valid slot name.
        /// </summary>
        /// <param name="slot">Slot name.</param>
        /// <returns>True when valid.</returns>
        public static bool IsValid(string slot) => slot != null && Pattern.IsMatch(slot);

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
