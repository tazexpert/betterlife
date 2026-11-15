using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Increments a counter flag. Story flags are hidden memory, so this produces no player-facing text.
    /// </summary>
    public sealed class IncrementFlagEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="flagId">Declared counter flag id.</param>
        /// <param name="amount">Positive amount.</param>
        /// <param name="expiryWeeks">Overrides the declared expiry, or null.</param>
        public IncrementFlagEffect(string flagId, int amount = 1, int? expiryWeeks = null)
        {
            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Amount must be positive.");
            }
            if (expiryWeeks.HasValue && expiryWeeks.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expiryWeeks), expiryWeeks, "Expiry must be positive.");
            }
            FlagId = ContentId.Require(flagId, nameof(flagId));
            Amount = amount;
            ExpiryWeeks = expiryWeeks;
        }

        /// <summary>Flag id.</summary>
        public string FlagId { get; }

        /// <summary>Amount added.</summary>
        public int Amount { get; }

        /// <summary>Expiry override, or null.</summary>
        public int? ExpiryWeeks { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Character.Flags.Increment(FlagId, context.Now, Amount, ExpiryWeeks);
            return EffectResult.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
