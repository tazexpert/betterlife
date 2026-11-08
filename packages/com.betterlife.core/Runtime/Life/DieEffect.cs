using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Ends the character's life with a cause (for fatal event outcomes). Sends a <see cref="DeathRequest"/>.
    /// </summary>
    public sealed class DieEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="causeId">Death cause id.</param>
        public DieEffect(string causeId)
        {
            CauseId = ContentId.Require(causeId, nameof(causeId));
        }

        /// <summary>Death cause id.</summary>
        public string CauseId { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return EffectResult.FromMessage(new DeathRequest(context.Character, CauseId, context.Now));
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
