using System;
using System.Globalization;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Flags;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Passes when a story flag is set, or when its count matches a comparison. Hidden from the player.
    /// </summary>
    public sealed class FlagCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="flagId">Declared flag id.</param>
        /// <param name="op">Comparison of the count, or null to require the flag to be set.</param>
        /// <param name="value">Count to compare with when <paramref name="op"/> is given.</param>
        public FlagCondition(string flagId, ComparisonOperator? op = null, int value = 0)
        {
            FlagId = ContentId.Require(flagId, nameof(flagId));
            Operator = op;
            Value = value;
        }

        /// <summary>Flag id.</summary>
        public string FlagId { get; }

        /// <summary>Count comparison, or null for "is set".</summary>
        public ComparisonOperator? Operator { get; }

        /// <summary>Count to compare with.</summary>
        public int Value { get; }
        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            FlagSet flags = context.Character.Flags;
            return Operator.HasValue ? Comparison.Compare(flags.GetCount(FlagId), Operator.Value, Value) : flags.IsSet(FlagId);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = Operator.HasValue
                ? RuleText.Of(RuleTextKeys.CondFlagCount, (RuleTextKeys.ArgSubject, FlagId), (RuleTextKeys.ArgOp, Comparison.ToSymbol(Operator.Value)), (RuleTextKeys.ArgValue, Value.ToString(CultureInfo.InvariantCulture)))
                : RuleText.Of(RuleTextKeys.CondFlagSet, (RuleTextKeys.ArgSubject, FlagId));
            return new ConditionTrace(Evaluate(context), text, false);
        }
    }
}
