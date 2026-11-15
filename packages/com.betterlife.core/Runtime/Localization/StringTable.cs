using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Localization
{
    /// <summary>
    /// All texts of one language.
    /// </summary>
    public sealed class StringTable
    {
        private readonly Dictionary<string, string> _strings;

        /// <summary>
        /// Creates the table.
        /// </summary>
        /// <param name="language">Language code (lowercase letters).</param>
        /// <param name="strings">Text per key.</param>
        public StringTable(string language, IReadOnlyDictionary<string, string> strings)
        {
            Language = ContentId.Require(language, nameof(language));
            _strings = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> pair in strings ?? throw new ArgumentNullException(nameof(strings)))
            {
                _strings[ContentId.RequireLocKey(pair.Key, nameof(strings))] = pair.Value ?? throw new ArgumentException($"Text of '{pair.Key}' is null.", nameof(strings));
            }
        }

        /// <summary>Language code.</summary>
        public string Language { get; }

        /// <summary>Keys with text.</summary>
        public IEnumerable<string> Keys => _strings.Keys;

        /// <summary>
        /// Returns the text of a key, or null.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <returns>Text or null.</returns>
        public string Find(string key) => key != null && _strings.TryGetValue(key, out string text) ? text : null;
    }
}
