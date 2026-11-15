using System;
using System.Collections.Generic;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Adds good karma silently (karma is shown on the astrology screen, not as an outcome line).
    /// </summary>
    public sealed class GoodKarmaEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="amount">Non-negative fixed or ranged amount.</param>
        public GoodKarmaEffect(FloatRange amount)
        {
            if (amount.Min < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount.Min, "Amount must be non-negative.");
            }
            Amount = amount;
        }

        /// <summary>Amount.</summary>
        public FloatRange Amount { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Character.Karma.AddGood(Amount.Roll(context.Random));
            return EffectResult.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
