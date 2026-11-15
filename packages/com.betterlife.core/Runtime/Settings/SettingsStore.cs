using System;
using BetterLife.Core.Saving;
using Newtonsoft.Json;

namespace BetterLife.Core.Settings
{
    /// <summary>
    /// Reads and writes <see cref="GameSettings"/> in its own profile slot.
    /// </summary>
    public sealed class SettingsStore
    {
        private readonly ISaveStorage _storage;

        /// <summary>
        /// Creates the store.
        /// </summary>
        /// <param name="storage">Profile storage.</param>
        public SettingsStore(ISaveStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        /// <summary>
        /// Loads the settings; a missing or unreadable slot gives the defaults.
        /// </summary>
        /// <returns>The settings.</returns>
        public GameSettings Load()
        {
            string text = _storage.Read(SlotNames.Settings);
            if (string.IsNullOrEmpty(text))
            {
                return new GameSettings();
            }
            try
            {
                return JsonConvert.DeserializeObject<GameSettings>(text) ?? new GameSettings();
            }
            catch (JsonException)
            {
                return new GameSettings();
            }
        }

        /// <summary>
        /// Saves the settings.
        /// </summary>
        /// <param name="settings">Settings.</param>
        public void Save(GameSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            _storage.Write(SlotNames.Settings, JsonConvert.SerializeObject(settings));
        }

        /// <summary>
        /// Picks the language to use: the saved choice when available, otherwise the device language when available,
        /// otherwise the fallback.
        /// </summary>
        /// <param name="saved">Saved language, or null.</param>
        /// <param name="device">Device language code, or null.</param>
        /// <param name="available">Languages with a string table.</param>
        /// <param name="fallback">Language used when nothing else fits.</param>
        /// <returns>The language code.</returns>
        public static string ResolveLanguage(string saved, string device, System.Collections.Generic.IEnumerable<string> available, string fallback)
        {
            var languages = new System.Collections.Generic.HashSet<string>(available ?? new string[0], StringComparer.Ordinal);
            if (saved != null && languages.Contains(saved))
            {
                return saved;
            }
            return device != null && languages.Contains(device) ? device : fallback;
        }
    }
}
