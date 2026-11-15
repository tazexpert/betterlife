using System;
using System.Globalization;
using BetterLife.Core.Localization;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Formats money for display through localized patterns ("฿{value}", "฿{value}M"...).
    /// </summary>
    public sealed class MoneyFormatter
    {
        /// <summary>Key of the full amount pattern.</summary>
        public const string FullKey = "money.full";
        /// <summary>Key of the thousands pattern.</summary>
        public const string ThousandKey = "money.compact.thousand";
        /// <summary>Key of the millions pattern.</summary>
        public const string MillionKey = "money.compact.million";
        /// <summary>Key of the billions pattern.</summary>
        public const string BillionKey = "money.compact.billion";
        /// <summary>Key of the negative amount pattern.</summary>
        public const string NegativeKey = "money.negative";

        private const decimal Thousand = 1000m;
        private const decimal Million = 1000000m;
        private const decimal Billion = 1000000000m;
        private const string FullFormat = "#,0";
        private const string CompactFormat = "0.#";

        private readonly ILocalizer _localizer;

        /// <summary>
        /// Creates the formatter.
        /// </summary>
        /// <param name="localizer">Localizer.</param>
        public MoneyFormatter(ILocalizer localizer)
        {
            _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        }

        /// <summary>
        /// Formats a whole amount with digit grouping (fractions are dropped).
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <returns>Text.</returns>
        public string Full(decimal amount)
        {
            string text = _localizer.Format(FullKey, ("value", Math.Abs(Math.Truncate(amount)).ToString(FullFormat, CultureInfo.InvariantCulture)));
            return Sign(amount, text);
        }

        /// <summary>
        /// Formats an amount in thousands, millions or billions with one decimal (below a thousand: the full amount).
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <returns>Text.</returns>
        public string Compact(decimal amount)
        {
            decimal size = Math.Abs(amount);
            if (size < Thousand)
            {
                return Full(amount);
            }
            string key = size >= Billion ? BillionKey : size >= Million ? MillionKey : ThousandKey;
            decimal unit = size >= Billion ? Billion : size >= Million ? Million : Thousand;
            decimal scaled = Math.Truncate(size / unit * 10m) / 10m;
            return Sign(amount, _localizer.Format(key, ("value", scaled.ToString(CompactFormat, CultureInfo.InvariantCulture))));
        }

        private string Sign(decimal amount, string text) => amount < 0m && Math.Truncate(amount) != 0m ? _localizer.Format(NegativeKey, ("value", text)) : text;
    }
}
