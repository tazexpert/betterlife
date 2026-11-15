using System;
using System.Globalization;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Compares the character's age in whole years.
    /// </summary>
    public sealed class AgeCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="op">Comparison.</param>
        /// <param name="years">Age in years; non-negative.</param>
        public AgeCondition(ComparisonOperator op, int years)
        {
            if (years < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(years), years, "Age must be non-negative.");
            }
            Operator = op;
            Years = years;
        }

        /// <summary>Comparison.</summary>
        public ComparisonOperator Operator { get; }

        /// <summary>Age in years.</summary>
        public int Years { get; }
        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return Comparison.Compare(context.Character.AgeAt(context.Now), Operator, Years);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = RuleText.Of(RuleTextKeys.CondAge,
                (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)), (RuleTextKeys.ArgValue, Years.ToString(CultureInfo.InvariantCulture)));
            return new ConditionTrace(Evaluate(context), text, true);
        }
    }
}
