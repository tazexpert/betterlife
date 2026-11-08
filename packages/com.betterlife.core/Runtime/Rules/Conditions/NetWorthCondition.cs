using System;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Compares net worth (cash + assets - debts) with an amount.
    /// </summary>
    public sealed class NetWorthCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="op">Comparison.</param>
        /// <param name="amount">Amount to compare with.</param>
        public NetWorthCondition(ComparisonOperator op, decimal amount)
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
            return Comparison.Compare(context.Character.NetWorth, Operator, Amount);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = RuleText.Of(RuleTextKeys.CondNetWorth,
                (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)), (RuleTextKeys.ArgValue, RuleText.Number(Amount)));
            return new ConditionTrace(Evaluate(context), text, true);
        }
    }
}
