using System;
using System.Globalization;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Legacy
{
    /// <summary>
    /// Compares the character's generation in the family line (1 = founder).
    /// </summary>
    public sealed class GenerationCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="op">Comparison.</param>
        /// <param name="generation">Generation to compare with (at least 1).</param>
        public GenerationCondition(ComparisonOperator op, int generation)
        {
            if (generation < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(generation), generation, "Generation starts at 1.");
            }
            Operator = op;
            Generation = generation;
        }

        /// <summary>Comparison.</summary>
        public ComparisonOperator Operator { get; }

        /// <summary>Generation to compare with.</summary>
        public int Generation { get; }

        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return Comparison.Compare(context.Character.Generation, Operator, Generation);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = RuleText.Of(RuleTextKeys.CondGeneration,
                (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)), (RuleTextKeys.ArgValue, Generation.ToString(CultureInfo.InvariantCulture)));
            return new ConditionTrace(Evaluate(context), text, true);
        }
    }
}
