using System;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Rules.Conditions
{
    /// <summary>
    /// Passes when the character currently has a health condition.
    /// </summary>
    public sealed class HealthConditionCondition : ICondition
    {
        /// <summary>
        /// Creates the condition.
        /// </summary>
        /// <param name="conditionId">Health condition id.</param>
        public HealthConditionCondition(string conditionId)
        {
            ConditionId = ContentId.Require(conditionId, nameof(conditionId));
        }

        /// <summary>Health condition id.</summary>
        public string ConditionId { get; }
        /// <inheritdoc />
        public bool Evaluate(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return context.Character.HasCondition(ConditionId);
        }

        /// <inheritdoc />
        public ConditionTrace Explain(RuleContext context)
        {
            RuleText text = RuleText.Of(RuleTextKeys.CondHealthCondition, (RuleTextKeys.ArgSubject, ConditionId));
            return new ConditionTrace(Evaluate(context), text, true);
        }
    }
}
