using System;
using System.Collections.Generic;
using BetterLife.Core.Events;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Retires the character now (early retirement) with the pension of the current level. The career service applies it.
    /// </summary>
    public sealed class RetireEffect : IEffect
    {
        /// <summary>Rule text of the retirement.</summary>
        public const string RetiredKey = "rule.effect.career_retire";

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
            return new EffectResult(new[] { RuleText.Of(RetiredKey) }, new Domain.Stats.StatChangeResult[0], 0m,
                new IGameEvent[] { new RetirementRequestedEvent(context.Now) });
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            return context?.Character.Career.IsEmployed == true ? new[] { RuleText.Of(RetiredKey) } : new RuleText[0];
        }
    }
}
