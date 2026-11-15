using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// This year's fortune in one area with its breakdown.
    /// </summary>
    public sealed class FortuneRow
    {
        /// <summary>
        /// Creates a row.
        /// </summary>
        /// <param name="area">Area name.</param>
        /// <param name="value">Fortune 0..100.</param>
        /// <param name="valueText">Fortune text.</param>
        /// <param name="parts">Where the value comes from.</param>
        public FortuneRow(string area, float value, string valueText, IReadOnlyList<ValueRow> parts)
        {
            Area = area;
            Value = value;
            ValueText = valueText;
            Parts = parts;
        }

        /// <summary>Area name.</summary>
        public string Area { get; }

        /// <summary>Fortune 0..100.</summary>
        public float Value { get; }

        /// <summary>Fortune text.</summary>
        public string ValueText { get; }

        /// <summary>Where the value comes from.</summary>
        public IReadOnlyList<ValueRow> Parts { get; }
    }
}
