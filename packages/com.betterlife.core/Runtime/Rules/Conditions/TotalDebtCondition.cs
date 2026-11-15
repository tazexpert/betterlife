using System;
using System.Linq;
using BetterLife.Core.Economy;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Compares the total balance owed, optionally of one debt kind, with an amount.
    /// </summary>
    public sealed class TotalDebtCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="op">Comparison.</param>
        /// <param name="amount">Amount to compare with.</param>
        /// <param name="kind">Only debts of this kind, or null for all.</param>
        public TotalDebtCondition(ComparisonOperator op, decimal amount, DebtKind? kind)
        {
            Operator = op;
            Amount = amount;
            Kind = kind;
        }

        /// <summary>Comparison.</summary>
        public ComparisonOperator Operator { get; }

        /// <summary>Amount to compare with.</summary>
        public decimal Amount { get; }

        /// <summary>Debt kind filter, or null for all debts.</summary>
        public DebtKind? Kind { get; }

        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            decimal owed = context.Character.Finances.Debts.Debts
                .Where(d => !Kind.HasValue || d.Kind == Kind.Value)
                .Sum(d => d.Balance);
            return Comparison.Compare(owed, Operator, Amount);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = Kind.HasValue
                ? RuleText.Of(RuleTextKeys.CondTotalDebtKind, (RuleTextKeys.ArgSubject, EconomyNames.NameOf(Kind.Value)),
                    (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)), (RuleTextKeys.ArgValue, RuleText.Number(Amount)))
                : RuleText.Of(RuleTextKeys.CondTotalDebt,
                    (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator)), (RuleTextKeys.ArgValue, RuleText.Number(Amount)));
            return new ConditionTrace(Evaluate(context), text, true);
        }
    }
}
