namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// A generic label/value row used by many screens (money lines, breakdowns, memories...).
    /// </summary>
    public sealed class ValueRow
    {
        /// <summary>
        /// Creates a row.
        /// </summary>
        /// <param name="label">Label.</param>
        /// <param name="value">Value text.</param>
        /// <param name="detail">Extra detail, or empty.</param>
        /// <param name="tone">How it reads for the character.</param>
        public ValueRow(string label, string value, string detail = "", RowTone tone = RowTone.Neutral)
        {
            Label = label;
            Value = value;
            Detail = detail ?? string.Empty;
            Tone = tone;
        }

        /// <summary>Label.</summary>
        public string Label { get; }

        /// <summary>Value text.</summary>
        public string Value { get; }

        /// <summary>Extra detail, or empty.</summary>
        public string Detail { get; }

        /// <summary>How it reads for the character.</summary>
        public RowTone Tone { get; }

        /// <summary>
        /// Tone of a signed amount.
        /// </summary>
        /// <param name="amount">Amount.</param>
        /// <returns>Good when positive, bad when negative.</returns>
        public static RowTone ToneOf(decimal amount) => amount > 0m ? RowTone.Good : amount < 0m ? RowTone.Bad : RowTone.Neutral;

        /// <summary>
        /// Tone of a signed number.
        /// </summary>
        /// <param name="amount">Number.</param>
        /// <returns>Good when positive, bad when negative.</returns>
        public static RowTone ToneOf(float amount) => amount > 0f ? RowTone.Good : amount < 0f ? RowTone.Bad : RowTone.Neutral;
    }
}
