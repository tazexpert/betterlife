using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Notices
{
    /// <summary>
    /// Recent news for the inbox, collected from the interrupts of each time skip (newest first, capped). Interrupts of
    /// events that wait for an answer are skipped because the inbox already lists them as decisions. Not saved: the
    /// lasting record is the event history and the timeline.
    /// </summary>
    public sealed class NoticeFeed
    {
        /// <summary>Default number of notices kept.</summary>
        public const int DefaultCapacity = 30;

        private readonly List<Notice> _items = new List<Notice>();

        /// <summary>
        /// Creates an empty feed.
        /// </summary>
        /// <param name="capacity">Number of notices kept.</param>
        public NoticeFeed(int capacity = DefaultCapacity)
        {
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be at least 1.");
            }
            Capacity = capacity;
        }

        /// <summary>Number of notices kept.</summary>
        public int Capacity { get; }

        /// <summary>Notices, newest first.</summary>
        public IReadOnlyList<Notice> Items => _items;

        /// <summary>
        /// Adds the interrupts of a time skip.
        /// </summary>
        /// <param name="result">Time skip result.</param>
        /// <param name="inbox">Events waiting for an answer after the skip.</param>
        public void Add(AdvanceResult result, IReadOnlyList<PendingEvent> inbox)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            IReadOnlyList<PendingEvent> pending = inbox ?? new PendingEvent[0];
            foreach (InterruptRequest interrupt in result.Interrupts.OrderBy(i => i.Week))
            {
                if (IsPendingDecision(interrupt, pending))
                {
                    continue;
                }
                _items.Insert(0, new Notice(interrupt.Severity, interrupt.ReasonKey, interrupt.SourceId, interrupt.Week));
            }
            if (_items.Count > Capacity)
            {
                _items.RemoveRange(Capacity, _items.Count - Capacity);
            }
        }

        /// <summary>
        /// Forgets all notices (for example when another life starts).
        /// </summary>
        public void Clear() => _items.Clear();

        /// <summary>
        /// Finds the pending event that stopped a time skip, so its dialog can open right away.
        /// </summary>
        /// <param name="result">Time skip result.</param>
        /// <param name="inbox">Events waiting for an answer.</param>
        /// <returns>The instance id, or null.</returns>
        public static int? StoppingDecision(AdvanceResult result, IReadOnlyList<PendingEvent> inbox)
        {
            InterruptRequest stop = result?.StoppedBy;
            if (stop == null || inbox == null)
            {
                return null;
            }
            PendingEvent match = inbox.FirstOrDefault(p => Matches(stop, p));
            return match?.InstanceId;
        }

        private static bool IsPendingDecision(InterruptRequest interrupt, IReadOnlyList<PendingEvent> inbox)
        {
            return inbox.Any(p => Matches(interrupt, p));
        }

        private static bool Matches(InterruptRequest interrupt, PendingEvent pending)
        {
            return pending.EventId == interrupt.SourceId && pending.FiredOn == interrupt.Week;
        }
    }
}
