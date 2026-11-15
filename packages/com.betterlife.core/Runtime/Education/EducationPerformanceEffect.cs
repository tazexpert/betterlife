using System;
using System.Collections.Generic;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// Changes school performance of the current enrollment (no effect when not studying). Produces no player-facing text.
    /// </summary>
    public sealed class EducationPerformanceEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="amount">Fixed or ranged delta.</param>
        public EducationPerformanceEffect(FloatRange amount)
        {
            Amount = amount;
        }

        /// <summary>Delta.</summary>
        public FloatRange Amount { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            float delta = Amount.Roll(context.Random);
            context.Character.Education.Current?.ChangePerformance(delta);
            return EffectResult.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
