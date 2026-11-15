using System;
using System.Collections.Generic;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Makes merit (tham bun): good karma plus partial payment of karmic debt. Shown to the player.
    /// </summary>
    public sealed class MeritEffect : IEffect
    {
        private readonly KarmaService _karma;

        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="karma">Karma service.</param>
        /// <param name="amount">Non-negative fixed or ranged amount.</param>
        public MeritEffect(KarmaService karma, FloatRange amount)
        {
            if (amount.Min < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount.Min, "Amount must be non-negative.");
            }
            _karma = karma ?? throw new ArgumentNullException(nameof(karma));
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
            float amount = Amount.Roll(context.Random);
            _karma.MakeMerit(context.Character, amount);
            return EffectResult.FromLine(RuleText.Of(KarmaTextKeys.EffectMerit, (RuleTextKeys.ArgValue, RuleText.Number(amount))));
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new[] { RangeText.Preview(KarmaTextKeys.EffectMerit, Amount) };
    }
}
