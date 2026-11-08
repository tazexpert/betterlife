using System;
using System.Collections.Generic;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Reads {"type":"event.schedule","event":id,"delayWeeks":4 or {"min":26,"max":52}}.
    /// Registered by the events loader with the set of all event ids, so references are checked across files.
    /// </summary>
    public sealed class ScheduleEventEffectConverter : EffectConverter<ScheduleEventEffect>
    {
        /// <summary>JSON type name.</summary>
        public const string TypeName = "event.schedule";

        private readonly ICollection<string> _knownEvents;

        /// <summary>
        /// Creates the converter.
        /// </summary>
        /// <param name="knownEvents">Every event id in the content.</param>
        public ScheduleEventEffectConverter(ICollection<string> knownEvents)
        {
            _knownEvents = knownEvents ?? throw new ArgumentNullException(nameof(knownEvents));
        }

        /// <inheritdoc />
        public override string Type => TypeName;

        /// <inheritdoc />
        protected override ScheduleEventEffect ReadTyped(JObject json, RuleReadContext context, RuleRegistry registry)
        {
            RuleJson.RequireOnly(json, context, "event", "delayWeeks");
            string eventId = RuleJson.RequireId(json, "event", context);
            if (!_knownEvents.Contains(eventId))
            {
                throw context.Property("event").Error($"unknown event '{eventId}'.");
            }

            if (json["delayWeeks"] is JObject range)
            {
                RuleReadContext rangeContext = context.Property("delayWeeks");
                RuleJson.RequireOnly(range, rangeContext, "min", "max");
                return new ScheduleEventEffect(eventId, RuleJson.RequireInt(range, "min", rangeContext), RuleJson.RequireInt(range, "max", rangeContext));
            }
            int weeks = RuleJson.RequireInt(json, "delayWeeks", context);
            return new ScheduleEventEffect(eventId, weeks, weeks);
        }

        /// <inheritdoc />
        protected override void WriteTyped(ScheduleEventEffect effect, JObject json, RuleRegistry registry)
        {
            json["event"] = effect.EventId;
            json["delayWeeks"] = effect.MinDelayWeeks == effect.MaxDelayWeeks
                ? (JToken)effect.MinDelayWeeks
                : new JObject { ["min"] = effect.MinDelayWeeks, ["max"] = effect.MaxDelayWeeks };
        }
    }
}
