using System;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Passes when its child fails.
    /// </summary>
    public sealed class NotCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="child">Condition to invert.</param>
        public NotCondition(ICondition child)
        {
            Child = child ?? throw new ArgumentNullException(nameof(child));
        }

        /// <summary>The inverted condition.</summary>
        public ICondition Child { get; }

        /// <inheritdoc />
        public bool Evaluate(RuleContext context) => !Child.Evaluate(context);

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            ConditionTrace child = Child.Explain(context);
            return new ConditionTrace(!child.Passed, new RuleText(RuleTextKeys.CondNot), child.IsVisibleToPlayer, new[] { child });
        }
    }
}
