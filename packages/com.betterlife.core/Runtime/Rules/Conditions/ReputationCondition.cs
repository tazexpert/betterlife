using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Compares the hidden reputation in a domain.
    /// </summary>
    public sealed class ReputationCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="domainId">Reputation domain id.</param>
        /// <param name="op">Comparison.</param>
        /// <param name="value">Value to compare with (0-100).</param>
        public ReputationCondition(string domainId, ComparisonOperator op, float value)
        {
            if (!StatBounds.IsInRange(value))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Value must be within 0-100.");
            }
            DomainId = ContentId.Require(domainId, nameof(domainId));
            Operator = op;
            Value = value;
        }

        /// <summary>Reputation domain id.</summary>
        public string DomainId { get; }

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
            return Comparison.Compare(context.Character.Reputation.Get(DomainId), Operator, Value);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = RuleText.Of(RuleTextKeys.CondReputation,
                (RuleTextKeys.ArgSubject, DomainId), (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)), (RuleTextKeys.ArgValue, RuleText.Number(Value)));
            return new ConditionTrace(Evaluate(context), text, false);
        }
    }
}
