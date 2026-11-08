using System;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Compares cash on hand with an amount.
    /// </summary>
    public sealed class CashCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="op">Comparison.</param>
        /// <param name="amount">Amount to compare with.</param>
        public CashCondition(ComparisonOperator op, decimal amount)
        {
            Operator = op;
            Amount = amount;
        }

        /// <summary>Comparison.</summary>
        public ComparisonOperator Operator { get; }

        /// <summary>Amount to compare with.</summary>
        public decimal Amount { get; }
        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return Comparison.Compare(context.Character.Wallet.Cash, Operator, Amount);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = RuleText.Of(RuleTextKeys.CondCash,
                (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)), (RuleTextKeys.ArgValue, RuleText.Number(Amount)));
            return new ConditionTrace(Evaluate(context), text, true);
        }
    }
}
