using System;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// Compares a measure of the character's habit with a value (a never-met vice counts as all zeros).
    /// </summary>
    public sealed class ViceCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <param name="measure">What to compare.</param>
        /// <param name="op">Comparison.</param>
        /// <param name="value">Value to compare with.</param>
        public ViceCondition(string viceId, ViceMeasure measure, ComparisonOperator op, float value)
        {
            ViceId = viceId ?? throw new ArgumentNullException(nameof(viceId));
            Measure = measure;
            Operator = op;
            Value = value;
        }

        /// <summary>Vice id.</summary>
        public string ViceId { get; }

        /// <summary>What to compare.</summary>
        public ViceMeasure Measure { get; }

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
            ViceState state = context.Character.Vices.Find(ViceId);
            float actual;
            switch (Measure)
            {
                case ViceMeasure.Addiction:
                    actual = state?.Addiction ?? 0f;
                    break;
                case ViceMeasure.Tolerance:
                    actual = state?.Tolerance ?? 0f;
                    break;
                case ViceMeasure.Uses:
                    actual = state?.Uses ?? 0;
                    break;
                default:
                    actual = state?.CleanWeeks(context.Now) ?? 0;
                    break;
            }
            return Comparison.Compare(actual, Operator, Value);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = RuleText.Of(RuleTextKeys.CondVice, (RuleTextKeys.ArgSubject, ViceId), (RuleTextKeys.ArgSubjectKey, ViceConditionConverter.NameOf(Measure)),
                (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)), (RuleTextKeys.ArgValue, RuleText.Number(Value)));
            return new ConditionTrace(Evaluate(context), text, false);
        }
    }
}
