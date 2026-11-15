using System;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Compares good or bad karma with a value. Hidden from the player.
    /// </summary>
    public sealed class KarmaCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="kind">Good or bad karma.</param>
        /// <param name="op">Comparison.</param>
        /// <param name="value">Value within 0-100.</param>
        public KarmaCondition(KarmaKind kind, ComparisonOperator op, float value)
        {
            if (!StatBounds.IsInRange(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within 0-100.");
            }
            Kind = kind;
            Operator = op;
            Value = value;
        }

        /// <summary>Good or bad karma.</summary>
        public KarmaKind Kind { get; }

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
            KarmaLedger ledger = context.Character.Karma;
            return Comparison.Compare(Kind == KarmaKind.Good ? ledger.Good : ledger.Bad, Operator, Value);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = RuleText.Of(KarmaTextKeys.CondKarma,
                (RuleTextKeys.ArgSubject, Kind == KarmaKind.Good ? "good" : "bad"),
                (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)),
                (RuleTextKeys.ArgValue, RuleText.Number(Value)));
            return new ConditionTrace(Evaluate(context), text, false);
        }
    }
}
