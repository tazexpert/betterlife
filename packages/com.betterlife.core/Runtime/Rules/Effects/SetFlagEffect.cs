using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Sets a bool flag. Story flags are hidden memory, so this produces no player-facing text.
    /// </summary>
    public sealed class SetFlagEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="flagId">Declared bool flag id.</param>
        /// <param name="expiryWeeks">Overrides the declared expiry, or null.</param>
        public SetFlagEffect(string flagId, int? expiryWeeks = null)
        {
            if (expiryWeeks.HasValue && expiryWeeks.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(expiryWeeks), expiryWeeks, "Expiry must be positive.");
            }
            FlagId = ContentId.Require(flagId, nameof(flagId));
            ExpiryWeeks = expiryWeeks;
        }

        /// <summary>Flag id.</summary>
        public string FlagId { get; }

        /// <summary>Expiry override, or null.</summary>
        public int? ExpiryWeeks { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Character.Flags.Set(FlagId, context.Now, ExpiryWeeks);
            return EffectResult.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
