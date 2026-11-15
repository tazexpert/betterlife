using System;
using System.Collections.Generic;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Karma
{
    /// <summary>
    /// Records a bad deed silently: bad karma now, karmic debt later.
    /// </summary>
    public sealed class BadKarmaEffect : IEffect
    {
        private readonly KarmaService _karma;

        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="karma">Karma service.</param>
        /// <param name="amount">Non-negative fixed or ranged amount.</param>
        public BadKarmaEffect(KarmaService karma, FloatRange amount)
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
            _karma.RecordBadDeed(context.Character, Amount.Roll(context.Random), context.SourceKey, context.Now, context.Random);
            return EffectResult.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
