using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Events;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Remembers the latest life events without choices (with their effects) so the story popup can show them.
    /// Not saved: a story is shown when it happens.
    /// </summary>
    public sealed class StoryLog : IDisposable
    {
        /// <summary>Default number of stories kept.</summary>
        public const int DefaultCapacity = 20;

        private readonly List<StoryEntry> _items = new List<StoryEntry>();
        private readonly IDisposable _subscription;

        /// <summary>
        /// Creates the log and listens for fired events.
        /// </summary>
        /// <param name="bus">Event bus of the life.</param>
        /// <param name="capacity">Number of stories kept.</param>
        public StoryLog(IEventBus bus, int capacity = DefaultCapacity)
        {
            if (bus == null)
            {
                throw new ArgumentNullException(nameof(bus));
            }
            if (capacity < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be at least 1.");
            }
            Capacity = capacity;
            _subscription = bus.Subscribe<LifeEventFiredEvent>(OnFired);
        }

        /// <summary>Number of stories kept.</summary>
        public int Capacity { get; }

        /// <summary>Stories, newest first.</summary>
        public IReadOnlyList<StoryEntry> Items => _items;

        /// <summary>
        /// Finds a story by event instance id.
        /// </summary>
        /// <param name="instanceId">Instance id.</param>
        /// <returns>The story, or null.</returns>
        public StoryEntry Find(int instanceId) => _items.FirstOrDefault(s => s.InstanceId == instanceId);

        /// <summary>
        /// Returns the story that stopped a time skip, if any.
        /// </summary>
        /// <param name="result">Result of the skip.</param>
        /// <returns>The story, or null.</returns>
        public StoryEntry Stopping(AdvanceResult result)
        {
            InterruptRequest stop = result?.StoppedBy;
            if (stop == null)
            {
                return null;
            }
            return _items.FirstOrDefault(s => s.Definition.Id == stop.SourceId && s.Week == stop.Week);
        }

        /// <summary>Stops listening.</summary>
        public void Dispose() => _subscription.Dispose();

        private void OnFired(LifeEventFiredEvent e)
        {
            if (e.Definition.HasChoices)
            {
                return;
            }
            _items.Insert(0, new StoryEntry(e.InstanceId, e.Definition, e.Week, e.OnFire));
            if (_items.Count > Capacity)
            {
                _items.RemoveAt(_items.Count - 1);
            }
        }
    }
}
