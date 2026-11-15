using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Localization
{
    /// <summary>
    /// String tables of every language, from content/strings/*.json.
    /// </summary>
    public sealed class StringTables
    {
        private readonly Dictionary<string, StringTable> _tables;

        /// <summary>
        /// Creates the set; languages must be unique.
        /// </summary>
        /// <param name="tables">Tables.</param>
        public StringTables(IEnumerable<StringTable> tables)
        {
            _tables = new Dictionary<string, StringTable>(StringComparer.Ordinal);
            foreach (StringTable table in tables ?? throw new ArgumentNullException(nameof(tables)))
            {
                if (_tables.ContainsKey(table.Language))
                {
                    throw new ArgumentException($"Language '{table.Language}' appears twice.", nameof(tables));
                }
                _tables.Add(table.Language, table);
            }
        }

        /// <summary>Language codes, sorted.</summary>
        public IReadOnlyList<string> Languages => _tables.Keys.OrderBy(k => k, StringComparer.Ordinal).ToList();

        /// <summary>
        /// Returns the table of a language, or null.
        /// </summary>
        /// <param name="language">Language code.</param>
        /// <returns>The table.</returns>
        public StringTable Find(string language) => language != null && _tables.TryGetValue(language, out StringTable table) ? table : null;
    }
}
