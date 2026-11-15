using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Rules
{
    /// <summary>
    /// Evaluates <see cref="ComparisonOperator"/> for numbers and converts operators to and from JSON symbols.
    /// </summary>
    public static class Comparison
    {
        /// <summary>Two floats closer than this are treated as equal.</summary>
        public const float FloatTolerance = 0.0001f;

        private static readonly Dictionary<ComparisonOperator, string> Symbols = new Dictionary<ComparisonOperator, string>
        {
            { ComparisonOperator.Equal, "==" },
            { ComparisonOperator.NotEqual, "!=" },
            { ComparisonOperator.Less, "<" },
            { ComparisonOperator.LessOrEqual, "<=" },
            { ComparisonOperator.Greater, ">" },
            { ComparisonOperator.GreaterOrEqual, ">=" },
        };

        /// <summary>
        /// Compares floats; values within <see cref="FloatTolerance"/> count as equal.
        /// </summary>
        /// <param name="left">Actual value.</param>
        /// <param name="op">Operator.</param>
        /// <param name="right">Expected value.</param>
        /// <returns>The comparison result.</returns>
        public static bool Compare(float left, ComparisonOperator op, float right)
        {
            int sign = Math.Abs(left - right) <= FloatTolerance ? 0 : left.CompareTo(right);
            return FromSign(sign, op);
        }

        /// <summary>
        /// Compares any comparable values exactly.
        /// </summary>
        /// <typeparam name="T">Value type.</typeparam>
        /// <param name="left">Actual value.</param>
        /// <param name="op">Operator.</param>
        /// <param name="right">Expected value.</param>
        /// <returns>The comparison result.</returns>
        public static bool Compare<T>(T left, ComparisonOperator op, T right) where T : IComparable<T>
        {
            return FromSign(left.CompareTo(right), op);
        }

        /// <summary>
        /// Returns the JSON symbol of an operator.
        /// </summary>
        /// <param name="op">Operator.</param>
        /// <returns>Symbol such as "&gt;=".</returns>
        public static string ToSymbol(ComparisonOperator op)
        {
            return Symbols[op];
        }

        /// <summary>
        /// Parses a JSON symbol.
        /// </summary>
        /// <param name="symbol">Symbol such as "&gt;=".</param>
        /// <param name="op">The operator when recognized.</param>
        /// <returns>True when recognized.</returns>
        public static bool TryParseSymbol(string symbol, out ComparisonOperator op)
        {
            foreach (KeyValuePair<ComparisonOperator, string> pair in Symbols.Where(p => p.Value == symbol))
            {
                op = pair.Key;
                return true;
            }
            op = default;
            return false;
        }

        private static bool FromSign(int sign, ComparisonOperator op)
        {
            switch (op)
            {
                case ComparisonOperator.Equal:
                    return sign == 0;
                case ComparisonOperator.NotEqual:
                    return sign != 0;
                case ComparisonOperator.Less:
                    return sign < 0;
                case ComparisonOperator.LessOrEqual:
                    return sign <= 0;
                case ComparisonOperator.Greater:
                    return sign > 0;
                case ComparisonOperator.GreaterOrEqual:
                    return sign >= 0;
                default:
                    throw new ArgumentOutOfRangeException(nameof(op), op, "Unknown operator.");
            }
        }
    }
}
