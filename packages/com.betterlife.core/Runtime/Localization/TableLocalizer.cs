using System;
using System.Text;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Localization
{
    /// <summary>
    /// Localizer backed by string tables: the chosen language first, then a fallback language, then the key itself.
    /// </summary>
    public sealed class TableLocalizer : ILocalizer
    {
        private readonly StringTable _primary;
        private readonly StringTable _fallback;

        /// <summary>
        /// Creates the localizer.
        /// </summary>
        /// <param name="tables">Every language.</param>
        /// <param name="language">Wanted language.</param>
        /// <param name="fallbackLanguage">Language used for missing keys, or null.</param>
        public TableLocalizer(StringTables tables, string language, string fallbackLanguage = null)
        {
            if (tables == null)
            {
                throw new ArgumentNullException(nameof(tables));
            }
            _primary = tables.Find(language) ?? tables.Find(fallbackLanguage);
            _fallback = tables.Find(fallbackLanguage);
            Language = _primary?.Language ?? language;
        }

        /// <inheritdoc />
        public string Language { get; }

        /// <inheritdoc />
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }
            return _primary?.Find(key) ?? _fallback?.Find(key) ?? key;
        }

        /// <inheritdoc />
        public string Format(string key, params (string Name, string Value)[] args)
        {
            string text = Get(key);
            if (args == null || args.Length == 0)
            {
                return text;
            }
            var builder = new StringBuilder(text);
            foreach ((string name, string value) in args)
            {
                builder.Replace("{" + name + "}", value ?? string.Empty);
            }
            return builder.ToString();
        }

        /// <inheritdoc />
        public string Format(RuleText text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }
            var args = new (string, string)[text.Args.Count];
            int i = 0;
            foreach (var pair in text.Args)
            {
                args[i++] = (pair.Key, pair.Value);
            }
            return Format(text.Key, args);
        }
    }
}
