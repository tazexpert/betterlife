using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Base of conditions that combine child conditions.
    /// </summary>
    public abstract class CompositeCondition : ICondition
    {
        /// <summary>
        /// Creates a composite.
        /// </summary>
        /// <param name="children">At least one child condition.</param>
        protected CompositeCondition(IEnumerable<ICondition> children)
        {
            if (children == null)
            {
                throw new ArgumentNullException(nameof(children));
            }
            Children = children.ToList();
            if (Children.Count == 0 || Children.Any(c => c == null))
            {
                throw new ArgumentException("A composite needs at least one non-null child.", nameof(children));
            }
        }

        /// <summary>Child conditions.</summary>
        public IReadOnlyList<ICondition> Children { get; }

        /// <summary>Localization key describing the combination.</summary>
        protected abstract string DescriptionKey { get; }

        /// <inheritdoc />
        public abstract bool Evaluate(RuleContext context);

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            List<ConditionTrace> traces = Children.Select(c => c.Explain(context)).ToList();
            return new ConditionTrace(Combine(traces.Select(t => t.Passed)), new RuleText(DescriptionKey), traces.Any(t => t.IsVisibleToPlayer), traces);
        }

        /// <summary>
        /// Combines child results.
        /// </summary>
        /// <param name="results">Child results.</param>
        /// <returns>The combined result.</returns>
        protected abstract bool Combine(IEnumerable<bool> results);
    }
}
