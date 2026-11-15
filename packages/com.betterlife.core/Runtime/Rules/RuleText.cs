using System;
using System.Collections.Generic;
using System.Globalization;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Rules
{
    /// <summary>
    /// Player-facing text as a localization key plus named arguments. Arguments whose name ends in "Key"
    /// are themselves localization keys (for example a stat name) and are resolved by the localizer.
    /// </summary>
    public sealed class RuleText
    {
        private const string NumberFormat = "0.##";

        /// <summary>
        /// Creates text.
        /// </summary>
        /// <param name="key">Localization key.</param>
        /// <param name="args">Named arguments.</param>
        public RuleText(string key, IReadOnlyDictionary<string, string> args = null)
        {
            Key = ContentId.RequireLocKey(key, nameof(key));
            Args = args ?? new Dictionary<string, string>();
        }

        /// <summary>Localization key.</summary>
        public string Key { get; }

        /// <summary>Named arguments.</summary>
        public IReadOnlyDictionary<string, string> Args { get; }

        /// <summary>
        /// Creates text from name/value pairs.
        /// </summary>
        /// <param name="key">Localization key.</param>
        /// <param name="args">Argument pairs.</param>
        /// <returns>The text.</returns>
        public static RuleText Of(string key, params (string Name, string Value)[] args)
        {
            var map = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach ((string name, string value) in args)
            {
                map[name] = value;
            }
            return new RuleText(key, map);
        }

        /// <summary>
        /// Formats a number for an argument (invariant culture, up to two decimals).
        /// </summary>
        /// <param name="value">Number.</param>
        /// <returns>Formatted number.</returns>
        public static string Number(float value) => value.ToString(NumberFormat, CultureInfo.InvariantCulture);

        /// <summary>
        /// Formats a money amount for an argument (invariant culture, up to two decimals).
        /// </summary>
        /// <param name="value">Amount.</param>
        /// <returns>Formatted amount.</returns>
        public static string Number(decimal value) => value.ToString(NumberFormat, CultureInfo.InvariantCulture);
    }
}
