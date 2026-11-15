using System;
using BetterLife.Core.Saving;
using Newtonsoft.Json;

namespace BetterLife.Core.Achievements
{
    /// <summary>
    /// Reads and writes the achievement profile in its own slot, separate from life saves (a new life keeps them).
    /// </summary>
    public sealed class AchievementStore
    {
        private readonly ISaveStorage _storage;

        /// <summary>
        /// Creates the store.
        /// </summary>
        /// <param name="storage">Profile storage.</param>
        public AchievementStore(ISaveStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <summary>
        /// Loads the profile; a missing or unreadable slot gives an empty profile.
        /// </summary>
        /// <returns>The profile.</returns>
        public AchievementProfile Load()
        {
            string text = _storage.Read(SlotNames.Achievements);
            if (string.IsNullOrEmpty(text))
            {
                return new AchievementProfile();
            }
            try
            {
                AchievementProfile profile = JsonConvert.DeserializeObject<AchievementProfile>(text) ?? new AchievementProfile();
                profile.Unlocked = profile.Unlocked ?? new System.Collections.Generic.List<AchievementUnlock>();
                return profile;
            }
            catch (JsonException)
            {
                return new AchievementProfile();
            }
        }

        /// <summary>
        /// Saves the profile.
        /// </summary>
        /// <param name="profile">Profile.</param>
        public void Save(AchievementProfile profile)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }
            _storage.Write(SlotNames.Achievements, JsonConvert.SerializeObject(profile));
        }
    }
}
