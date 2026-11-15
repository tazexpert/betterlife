using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Passes when every child passes (short-circuits).
    /// </summary>
    public sealed class AllCondition : CompositeCondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="children">Child conditions.</param>
        public AllCondition(IEnumerable<ICondition> children) : base(children)
        {
        }

        /// <inheritdoc />
        protected override string DescriptionKey => RuleTextKeys.CondAll;

        /// <inheritdoc />
        public override bool Evaluate(RuleContext context) => Children.All(c => c.Evaluate(context));

        /// <inheritdoc />
        protected override bool Combine(IEnumerable<bool> results) => results.All(r => r);
    }
}
