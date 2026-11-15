using System;
using System.Collections.Generic;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Changes work performance of the current job (no effect when unemployed). Produces no player-facing text.
    /// </summary>
    public sealed class CareerPerformanceEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="amount">Fixed or ranged delta.</param>
        public CareerPerformanceEffect(FloatRange amount)
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
            Job job = context.Character.Career.Current;
            job?.ChangePerformance(delta);
            return EffectResult.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
