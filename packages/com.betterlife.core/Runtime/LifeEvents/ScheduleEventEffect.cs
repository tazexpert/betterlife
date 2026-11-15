using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Delayed consequence: schedules an event a number of weeks from now. Silent, so the future stays a surprise.
    /// </summary>
    public sealed class ScheduleEventEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="eventId">Event to schedule.</param>
        /// <param name="minDelayWeeks">Earliest delay; at least 1.</param>
        /// <param name="maxDelayWeeks">Latest delay; at least the minimum.</param>
        public ScheduleEventEffect(string eventId, int minDelayWeeks, int maxDelayWeeks)
        {
            if (minDelayWeeks < 1 || maxDelayWeeks < minDelayWeeks)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDelayWeeks), maxDelayWeeks, "Delay must satisfy 1 <= min <= max.");
            }
            EventId = ContentId.Require(eventId, nameof(eventId));
            MinDelayWeeks = minDelayWeeks;
            MaxDelayWeeks = maxDelayWeeks;
        }

        /// <summary>Event to schedule.</summary>
        public string EventId { get; }

        /// <summary>Earliest delay.</summary>
        public int MinDelayWeeks { get; }

        /// <summary>Latest delay.</summary>
        public int MaxDelayWeeks { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            int delay = MinDelayWeeks == MaxDelayWeeks ? MinDelayWeeks : context.Random.NextInt(MinDelayWeeks, MaxDelayWeeks + 1);
            return EffectResult.FromMessage(new ScheduleEventRequest(EventId, context.Now.AddWeeks(delay)));
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context) => new RuleText[0];
    }
}
