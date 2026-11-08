using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Clears a flag of either kind. Story flags are hidden memory, so this produces no player-facing text.
    /// </summary>
    public sealed class ClearFlagEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="flagId">Declared flag id.</param>
        public ClearFlagEffect(string flagId)
        {
            FlagId = ContentId.Require(flagId, nameof(flagId));
        }

        /// <summary>Flag id.</summary>
        public string FlagId { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Character.Flags.Clear(FlagId);
            return EffectResult.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
