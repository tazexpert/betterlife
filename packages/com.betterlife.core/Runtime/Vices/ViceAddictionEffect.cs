using System;
using System.Collections.Generic;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// Changes the addiction to a vice (peer pressure, encouragement...). Produces no player-facing text.
    /// </summary>
    public sealed class ViceAddictionEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <param name="amount">Fixed or ranged change.</param>
        public ViceAddictionEffect(string viceId, FloatRange amount)
        {
            ViceId = viceId ?? throw new ArgumentNullException(nameof(viceId));
            Amount = amount;
        }

        /// <summary>Vice id.</summary>
        public string ViceId { get; }

        /// <summary>Change.</summary>
        public FloatRange Amount { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Character.Vices.Get(ViceId).ChangeAddiction(Amount.Roll(context.Random));
            return EffectResult.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
