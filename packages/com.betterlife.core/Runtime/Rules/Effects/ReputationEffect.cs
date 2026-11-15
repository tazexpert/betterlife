using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Changes the hidden reputation in a domain. Produces no player-facing text.
    /// </summary>
    public sealed class ReputationEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="domainId">Reputation domain id.</param>
        /// <param name="amount">Fixed or ranged delta.</param>
        public ReputationEffect(string domainId, FloatRange amount)
        {
            DomainId = ContentId.Require(domainId, nameof(domainId));
            Amount = amount;
        }

        /// <summary>Domain id.</summary>
        public string DomainId { get; }

        /// <summary>Delta.</summary>
        public FloatRange Amount { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            context.Character.Reputation.Change(DomainId, Amount.Roll(context.Random));
            return EffectResult.Empty;
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
