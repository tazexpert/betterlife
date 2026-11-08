using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Rules
{
    /// <summary>
    /// Builds preview text arguments for fixed values and ranges.
    /// </summary>
    public static class RangeText
    {
        /// <summary>
        /// Returns preview text with "value" for fixed amounts or "min"/"max" for ranges.
        /// </summary>
        /// <param name="key">Localization key.</param>
        /// <param name="range">The amount.</param>
        /// <param name="extra">Additional arguments.</param>
        /// <returns>The text.</returns>
        public static RuleText Preview(string key, FloatRange range, params (string Name, string Value)[] extra)
        {
            return RuleText.Of(key, extra.Concat(Args(range.IsFixed, RuleText.Number(range.Min), RuleText.Number(range.Max))).ToArray());
        }

        /// <summary>
        /// Returns preview text with "value" for fixed amounts or "min"/"max" for ranges.
        /// </summary>
        /// <param name="key">Localization key.</param>
        /// <param name="range">The amount.</param>
        /// <param name="extra">Additional arguments.</param>
        /// <returns>The text.</returns>
        public static RuleText Preview(string key, DecimalRange range, params (string Name, string Value)[] extra)
        {
            return RuleText.Of(key, extra.Concat(Args(range.IsFixed, RuleText.Number(range.Min), RuleText.Number(range.Max))).ToArray());
        }

        private static IEnumerable<(string, string)> Args(bool isFixed, string min, string max)
        {
            if (isFixed)
            {
                return new[] { (RuleTextKeys.ArgValue, min) };
            }
            return new[] { (RuleTextKeys.ArgMin, min), (RuleTextKeys.ArgMax, max) };
        }
    }
}
