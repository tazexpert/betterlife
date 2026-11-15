using System;
using System.Globalization;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Compares the current in-game calendar year, for era-specific content.
    /// </summary>
    public sealed class YearCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="op">Comparison.</param>
        /// <param name="year">Calendar year (Common Era).</param>
        public YearCondition(ComparisonOperator op, int year)
        {
            Operator = op;
            Year = year;
        }

        /// <summary>Comparison.</summary>
        public ComparisonOperator Operator { get; }

        /// <summary>Calendar year.</summary>
        public int Year { get; }
        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return Comparison.Compare(context.Now.Year, Operator, Year);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = RuleText.Of(RuleTextKeys.CondYear,
                (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)), (RuleTextKeys.ArgValue, Year.ToString(CultureInfo.InvariantCulture)));
            return new ConditionTrace(Evaluate(context), text, false);
        }
    }
}
