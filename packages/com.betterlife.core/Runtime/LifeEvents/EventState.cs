using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Time;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Everything the event system remembers about one life. Saved with the game.
    /// </summary>
    public sealed class EventState
    {
        /// <summary>Recent tones kept for anti-frustration.</summary>
        public const int MaxToneEntries = 16;

        private readonly Dictionary<string, EventRecord> _records = new Dictionary<string, EventRecord>(StringComparer.Ordinal);
        private readonly List<ScheduledEvent> _scheduled = new List<ScheduledEvent>();
        private readonly List<PendingEvent> _inbox = new List<PendingEvent>();
        private readonly List<ToneEntry> _tones = new List<ToneEntry>();
        private readonly List<EventHistoryEntry> _history = new List<EventHistoryEntry>();

        /// <summary>Per-event records.</summary>
        public IReadOnlyCollection<EventRecord> Records => _records.Values;

        /// <summary>Scheduled events, earliest first.</summary>
        public IReadOnlyList<ScheduledEvent> Scheduled => _scheduled;

        /// <summary>Events waiting for a choice, oldest first.</summary>
        public IReadOnlyList<PendingEvent> Inbox => _inbox;

        /// <summary>Recent tones, oldest first.</summary>
        public IReadOnlyList<ToneEntry> Tones => _tones;

        /// <summary>Finished occurrences, oldest first.</summary>
        public IReadOnlyList<EventHistoryEntry> History => _history;

        /// <summary>Id the next occurrence will get.</summary>
        public int NextInstanceId { get; private set; } = 1;

        /// <summary>
        /// Returns the record of an event, or null when it never occurred.
        /// </summary>
        /// <param name="eventId">Event id.</param>
        /// <returns>The record or null.</returns>
        public EventRecord FindRecord(string eventId) => eventId != null && _records.TryGetValue(eventId, out EventRecord record) ? record : null;

        /// <summary>
        /// Returns a pending event by occurrence id, or null.
        /// </summary>
        /// <param name="instanceId">Occurrence id.</param>
        /// <returns>The pending event or null.</returns>
        public PendingEvent FindPending(int instanceId) => _inbox.FirstOrDefault(p => p.InstanceId == instanceId);

        /// <summary>
        /// Schedules an event.
        /// </summary>
        /// <param name="scheduled">The scheduled event.</param>
        public void Schedule(ScheduledEvent scheduled)
        {
            if (scheduled == null)
            {
                throw new ArgumentNullException(nameof(scheduled));
            }
            int index = _scheduled.FindIndex(s => s.DueOn > scheduled.DueOn);
            _scheduled.Insert(index < 0 ? _scheduled.Count : index, scheduled);
        }

        internal int NewInstanceId() => NextInstanceId++;

        internal IReadOnlyList<ScheduledEvent> TakeDue(GameDate week)
        {
            List<ScheduledEvent> due = _scheduled.Where(s => s.DueOn <= week).ToList();
            _scheduled.RemoveAll(s => s.DueOn <= week);
            return due;
        }

        internal void RecordFire(string eventId, EventTone tone, GameDate week)
        {
            if (_records.TryGetValue(eventId, out EventRecord record))
            {
                record.RecordFire(week);
            }
            else
            {
                _records.Add(eventId, new EventRecord(eventId, week, 1));
            }
            AddTone(new ToneEntry(week, tone));
        }

        internal void AddPending(PendingEvent pending) => _inbox.Add(pending);

        internal void RemovePending(PendingEvent pending) => _inbox.Remove(pending);

        internal void AddHistory(EventHistoryEntry entry) => _history.Add(entry);

        /// <summary>
        /// Restores saved state, replacing everything.
        /// </summary>
        /// <param name="records">Records.</param>
        /// <param name="scheduled">Scheduled events.</param>
        /// <param name="inbox">Pending events.</param>
        /// <param name="tones">Recent tones.</param>
        /// <param name="history">History.</param>
        /// <param name="nextInstanceId">Next occurrence id; at least 1 and above every saved id.</param>
        public void Restore(
            IEnumerable<EventRecord> records,
            IEnumerable<ScheduledEvent> scheduled,
            IEnumerable<PendingEvent> inbox,
            IEnumerable<ToneEntry> tones,
            IEnumerable<EventHistoryEntry> history,
            int nextInstanceId)
        {
            _records.Clear();
            _scheduled.Clear();
            _inbox.Clear();
            _tones.Clear();
            _history.Clear();
            foreach (EventRecord record in records ?? Enumerable.Empty<EventRecord>())
            {
                _records[record.EventId] = record;
            }
            foreach (ScheduledEvent item in scheduled ?? Enumerable.Empty<ScheduledEvent>())
            {
                Schedule(item);
            }
            _inbox.AddRange(inbox ?? Enumerable.Empty<PendingEvent>());
            foreach (ToneEntry tone in tones ?? Enumerable.Empty<ToneEntry>())
            {
                AddTone(tone);
            }
            _history.AddRange(history ?? Enumerable.Empty<EventHistoryEntry>());
            int highest = _inbox.Select(p => p.InstanceId).Concat(_history.Select(h => h.InstanceId)).DefaultIfEmpty(0).Max();
            NextInstanceId = Math.Max(Math.Max(1, nextInstanceId), highest + 1);
        }

        private void AddTone(ToneEntry tone)
        {
            _tones.Add(tone);
            if (_tones.Count > MaxToneEntries)
            {
                _tones.RemoveAt(0);
            }
        }
    }
}
