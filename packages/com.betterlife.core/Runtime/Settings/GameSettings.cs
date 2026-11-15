using System;

namespace BetterLife.Core.Settings
{
    /// <summary>
    /// The player's preferences (kept in a profile slot, shared by all lives).
    /// </summary>
    [Serializable]
    public sealed class GameSettings
    {
        /// <summary>Language code ("en", "th"), or null to follow the device.</summary>
        public string Language;

        /// <summary>Whether vice content (drinking, gambling...) appears.</summary>
        public bool ViceContent = true;
    }
}
