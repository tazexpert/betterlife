using System;
using System.Collections.Generic;
using BetterLife.Core.Events;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Promotes the character one level in the current job. The career service applies it (it knows the levels);
    /// at the top level nothing changes.
    /// </summary>
    public sealed class PromoteEffect : IEffect
    {
        /// <summary>Rule text of the promotion.</summary>
        public const string PromotedKey = "rule.effect.career_promote";

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (!context.Character.Career.IsEmployed)
            {
                return EffectResult.Empty;
            }
            return new EffectResult(new[] { RuleText.Of(PromotedKey) }, new Domain.Stats.StatChangeResult[0], 0m,
                new IGameEvent[] { new PromotionRequestedEvent(context.Now) });
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            return context?.Character.Career.IsEmployed == true ? new[] { RuleText.Of(PromotedKey) } : new RuleText[0];
        }
    }
}
