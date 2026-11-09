using System;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// Compares the character's history with scammers.
    /// </summary>
    public sealed class ScamCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="measure">What to compare.</param>
        /// <param name="op">Comparison.</param>
        /// <param name="value">Value to compare with.</param>
        public ScamCondition(ScamMeasure measure, ComparisonOperator op, decimal value)
        {
            Measure = measure;
            Operator = op;
            Value = value;
        }

        /// <summary>What to compare.</summary>
        public ScamMeasure Measure { get; }

        /// <summary>Comparison.</summary>
        public ComparisonOperator Operator { get; }

        /// <summary>Value to compare with.</summary>
        public decimal Value { get; }

        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            CharacterScams scams = context.Character.Scams;
            decimal actual = Measure == ScamMeasure.Victims ? scams.Victimized : Measure == ScamMeasure.Detected ? scams.Detected : scams.NetLost;
            return Comparison.Compare(actual, Operator, Value);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = RuleText.Of(RuleTextKeys.CondScam, (RuleTextKeys.ArgSubject, ScamConditionConverter.NameOf(Measure)),
                (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)), (RuleTextKeys.ArgValue, RuleText.Number(Value)));
            return new ConditionTrace(Evaluate(context), text, false);
        }
    }
}
