using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Time;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Converts <see cref="EventState"/> to and from <see cref="EventStateDto"/>.
    /// Records, schedules and inbox entries of removed events are skipped with a warning; history is kept as is.
    /// </summary>
    public static class EventStateDtoMapper
    {
        /// <summary>
        /// Captures an event state.
        /// </summary>
        /// <param name="state">State to save.</param>
        /// <returns>The DTO.</returns>
        public static EventStateDto ToDto(EventState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            return new EventStateDto
            {
                Records = state.Records.OrderBy(r => r.EventId, StringComparer.Ordinal)
                    .Select(r => new EventRecordDto { EventId = r.EventId, LastFiredWeek = r.LastFiredOn.TotalWeeks, Occurrences = r.Occurrences }).ToList(),
                Scheduled = state.Scheduled.Select(s => new ScheduledEventDto { EventId = s.EventId, DueWeek = s.DueOn.TotalWeeks }).ToList(),
                Inbox = state.Inbox.Select(p => new PendingEventDto
                {
                    InstanceId = p.InstanceId, EventId = p.EventId, Origin = p.Origin.ToString(), FiredWeek = p.FiredOn.TotalWeeks, DeadlineWeek = p.Deadline.TotalWeeks,
                }).ToList(),
                Tones = state.Tones.Select(t => new ToneEntryDto { Week = t.Week.TotalWeeks, Tone = t.Tone.ToString() }).ToList(),
                History = state.History.Select(h => new EventHistoryDto
                {
                    InstanceId = h.InstanceId, EventId = h.EventId, Origin = h.Origin.ToString(), Week = h.Week.TotalWeeks, ChoiceId = h.ChoiceId, Succeeded = h.Succeeded,
                }).ToList(),
                NextInstanceId = state.NextInstanceId,
            };
        }

        /// <summary>
        /// Rebuilds an event state.
        /// </summary>
        /// <param name="dto">Saved state.</param>
        /// <param name="events">Current event catalog.</param>
        /// <returns>The state and warnings.</returns>
        public static EventStateLoadResult FromDto(EventStateDto dto, DefinitionCatalog<LifeEventDefinition> events)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            if (events == null)
            {
                throw new ArgumentNullException(nameof(events));
            }

            var warnings = new List<string>();
            bool Known(string id, string what)
            {
                if (events.Contains(id))
                {
                    return true;
                }
                warnings.Add($"Skipped {what} of removed event '{id}'.");
                return false;
            }

            var state = new EventState();
            state.Restore(
                Items(dto.Records).Where(r => Known(r.EventId, "record")).Select(r => new EventRecord(r.EventId, new GameDate(r.LastFiredWeek), r.Occurrences)),
                Items(dto.Scheduled).Where(s => Known(s.EventId, "schedule")).Select(s => new ScheduledEvent(s.EventId, new GameDate(s.DueWeek))),
                Items(dto.Inbox).Where(p => Known(p.EventId, "inbox entry"))
                    .Select(p => new PendingEvent(p.InstanceId, p.EventId, Parse<EventOrigin>(p.Origin), new GameDate(p.FiredWeek), new GameDate(p.DeadlineWeek))),
                Items(dto.Tones).Select(t => new ToneEntry(new GameDate(t.Week), Parse<EventTone>(t.Tone))),
                Items(dto.History).Select(h => new EventHistoryEntry(h.InstanceId, h.EventId, Parse<EventOrigin>(h.Origin), new GameDate(h.Week), h.ChoiceId, h.Succeeded)),
                dto.NextInstanceId);
            return new EventStateLoadResult(state, warnings);
        }

        private static IEnumerable<T> Items<T>(List<T> list) where T : class => (list ?? new List<T>()).Where(i => i != null);

        private static TEnum Parse<TEnum>(string value) where TEnum : struct
        {
            if (string.IsNullOrEmpty(value) || !Enum.TryParse(value, false, out TEnum result) || !Enum.IsDefined(typeof(TEnum), result))
            {
                throw new FormatException($"'{value}' is not a valid {typeof(TEnum).Name}.");
            }
            return result;
        }
    }
}
