using System;
using System.Collections.Generic;
using BetterLife.Core.Events;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Marries the closest living partner (the partner becomes the spouse). Nothing happens without a partner.
    /// </summary>
    public sealed class MarryEffect : IEffect
    {
        /// <summary>Rule text of the wedding.</summary>
        public const string MarriedKey = "rule.effect.marry";

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            Relationship partner = context.Character.Relationships.Closest(RelationshipType.Partner);
            if (partner == null)
            {
                return EffectResult.Empty;
            }
            partner.Become(RelationshipType.Spouse, context.Now);
            return new EffectResult(new[] { RuleText.Of(MarriedKey) }, new Domain.Stats.StatChangeResult[0], 0m,
                new IGameEvent[] { new RelationshipTypeChangedEvent(partner, RelationshipType.Partner, RelationshipType.Spouse, context.Now) });
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            return context?.Character.Relationships.Closest(RelationshipType.Partner) != null ? new[] { RuleText.Of(MarriedKey) } : new RuleText[0];
        }
    }
}
