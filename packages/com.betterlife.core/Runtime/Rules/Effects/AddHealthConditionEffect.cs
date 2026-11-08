using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Gives the character a health condition (replacing one with the same id).
    /// </summary>
    public sealed class AddHealthConditionEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="conditionId">Health condition id.</param>
        /// <param name="severity">Fixed or ranged severity within 0-100.</param>
        public AddHealthConditionEffect(string conditionId, FloatRange severity)
        {
            if (!StatBounds.IsInRange(severity.Min) || !StatBounds.IsInRange(severity.Max))
            {
                throw new ArgumentOutOfRangeException(nameof(severity), severity.Max, "Severity must be within 0-100.");
            }
            ConditionId = ContentId.Require(conditionId, nameof(conditionId));
            Severity = severity;
        }

        /// <summary>Health condition id.</summary>
        public string ConditionId { get; }

        /// <summary>Severity.</summary>
        public FloatRange Severity { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Character.SetCondition(new HealthCondition(ConditionId, Severity.Roll(context.Random), context.Now));
            return EffectResult.FromLine(Text());
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new[] { Text() };

        private RuleText Text() => RuleText.Of(RuleTextKeys.EffectHealthConditionAdd, (RuleTextKeys.ArgSubject, ConditionId));
    }
}
