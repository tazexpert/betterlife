using System;
using System.Collections.Generic;
using BetterLife.Core.Events;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Ends the current job for a reason other than being fired: the character quits, or is laid off.
    /// </summary>
    public sealed class EndCareerEffect : IEffect
    {
        /// <summary>Rule text when the character quits.</summary>
        public const string QuitKey = "rule.effect.career_quit";
        /// <summary>Rule text when the character is laid off.</summary>
        public const string LaidOffKey = "rule.effect.career_laid_off";

        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="reason">Quit or laid off.</param>
        public EndCareerEffect(JobEndReason reason)
        {
            if (reason != JobEndReason.Quit && reason != JobEndReason.LaidOff)
            {
                throw new ArgumentOutOfRangeException(nameof(reason), reason, "Only quitting and lay-offs are supported.");
            }
            Reason = reason;
        }

        /// <summary>Why the job ends.</summary>
        public JobEndReason Reason { get; }

        private string Key => Reason == JobEndReason.Quit ? QuitKey : LaidOffKey;

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            PastJob ended = context.Character.Career.End(Reason, context.Now);
            if (ended == null)
            {
                return EffectResult.Empty;
            }
            return new EffectResult(new[] { RuleText.Of(Key) }, new Domain.Stats.StatChangeResult[0], 0m,
                new IGameEvent[] { new JobEndedEvent(ended, context.Now) });
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            return context?.Character.Career.IsEmployed == true ? new[] { RuleText.Of(Key) } : new RuleText[0];
        }
    }
}
