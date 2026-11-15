using System;
using System.Collections.Generic;
using BetterLife.Core.Events;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Ends the current job as a firing and sets the fired flag (no effect when unemployed).
    /// </summary>
    public sealed class FireEffect : IEffect
    {
        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            PastJob ended = context.Character.Career.End(JobEndReason.Fired, context.Now);
            if (ended == null)
            {
                return EffectResult.Empty;
            }
            if (context.Character.Catalogs.Flags.Contains(CareerService.FiredFlag))
            {
                context.Character.Flags.Set(CareerService.FiredFlag, context.Now);
            }
            return new EffectResult(new[] { RuleText.Of(RuleTextKeys.EffectCareerFired) }, new Domain.Stats.StatChangeResult[0], 0m,
                new IGameEvent[] { new JobEndedEvent(ended, context.Now) });
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            return context?.Character.Career.IsEmployed == true ? new[] { RuleText.Of(RuleTextKeys.EffectCareerFired) } : new RuleText[0];
        }
    }
}
