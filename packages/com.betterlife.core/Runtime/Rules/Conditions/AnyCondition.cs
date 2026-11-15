using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Passes when at least one child passes (short-circuits).
    /// </summary>
    public sealed class AnyCondition : CompositeCondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="children">Child conditions.</param>
        public AnyCondition(IEnumerable<ICondition> children) : base(children)
        {
        }

        /// <inheritdoc />
        protected override string DescriptionKey => RuleTextKeys.CondAny;

        /// <inheritdoc />
        public override bool Evaluate(RuleContext context) => Children.Any(c => c.Evaluate(context));

        /// <inheritdoc />
        protected override bool Combine(IEnumerable<bool> results) => results.Any(r => r);
    }
}
