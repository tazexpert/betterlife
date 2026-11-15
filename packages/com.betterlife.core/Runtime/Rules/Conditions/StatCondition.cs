using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Compares a stat (stored or derived) with a value.
    /// </summary>
    public sealed class StatCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="statId">Stat to read.</param>
        /// <param name="op">Comparison.</param>
        /// <param name="value">Value to compare with (0-100).</param>
        public StatCondition(StatId statId, ComparisonOperator op, float value)
        {
            if (!StatBounds.IsInRange(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within stat bounds.");
            }
            StatId = statId;
            Operator = op;
            Value = value;
        }

        /// <summary>Stat to read.</summary>
        public StatId StatId { get; }

        /// <summary>Comparison.</summary>
        public ComparisonOperator Operator { get; }

        /// <summary>Value to compare with.</summary>
        public float Value { get; }
        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return Comparison.Compare(context.Character.Stats.GetValue(StatId), Operator, Value);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            StatDefinition definition = context.Character.Catalogs.Stats.GetDefinition(StatId);
            RuleText text = RuleText.Of(RuleTextKeys.CondStat,
                (RuleTextKeys.ArgSubjectKey, definition.NameKey),
                (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)),
                (RuleTextKeys.ArgValue, RuleText.Number(Value)));
            return new ConditionTrace(Evaluate(context), text, definition.Layer != StatLayer.Hidden);
        }
    }
}
