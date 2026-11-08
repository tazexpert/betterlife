using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Cures a health condition if present.
    /// </summary>
    public sealed class RemoveHealthConditionEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="conditionId">Health condition id.</param>
        public RemoveHealthConditionEffect(string conditionId)
        {
            ConditionId = ContentId.Require(conditionId, nameof(conditionId));
        }

        /// <summary>Health condition id.</summary>
        public string ConditionId { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return context.Character.RemoveCondition(ConditionId) ? EffectResult.FromLine(Text()) : EffectResult.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return context.Character.HasCondition(ConditionId) ? new[] { Text() } : new RuleText[0];
        }

        private RuleText Text() => RuleText.Of(RuleTextKeys.EffectHealthConditionRemove, (RuleTextKeys.ArgSubject, ConditionId));
    }
}
