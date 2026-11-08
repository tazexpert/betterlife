using System;
using System.Collections.Generic;
using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Weekly samples of tracked stats for dashboard sparklines. Keeps a fixed number of most recent samples.
    /// </summary>
    public sealed class StatHistory
    {
        /// <summary>Default number of weekly samples kept.</summary>
        public const int DefaultCapacity = 52;

        private readonly Dictionary<StatId, List<float>> _samples = new Dictionary<StatId, List<float>>();
        private readonly List<StatId> _tracked;
        private GameDate? _lastRecordedWeek;

        /// <summary>
        /// Creates a history.
        /// </summary>
        /// <param name="trackedStats">Stats to sample.</param>
        /// <param name="capacity">Samples kept per stat; must be positive.</param>
        public StatHistory(IEnumerable<StatId> trackedStats, int capacity = DefaultCapacity)
        {
            if (trackedStats == null)
            {
                throw new ArgumentNullException(nameof(trackedStats));
            }
            if (capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(capacity), capacity, "Capacity must be positive.");
            }

            Capacity = capacity;
            _tracked = new List<StatId>(trackedStats);
            foreach (StatId id in _tracked)
            {
                _samples[id] = new List<float>();
            }
        }

        /// <summary>Samples kept per stat.</summary>
        public int Capacity { get; }

        /// <summary>Stats that are sampled.</summary>
        public IReadOnlyList<StatId> TrackedStats => _tracked;

        /// <summary>Week of the latest sample, or null when empty.</summary>
        public GameDate? LastRecordedWeek => _lastRecordedWeek;

        /// <summary>
        /// Records one sample per tracked stat. Recording the same week twice replaces that week's sample.
        /// </summary>
        /// <param name="week">Current week; must not be earlier than the last recorded week.</param>
        /// <param name="stats">Source of values.</param>
        public void Record(GameDate week, IStatReader stats)
        {
            if (stats == null)
            {
                throw new ArgumentNullException(nameof(stats));
            }
            if (_lastRecordedWeek.HasValue && week < _lastRecordedWeek.Value)
            {
                throw new ArgumentOutOfRangeException(nameof(week), week, "Cannot record a week earlier than the last sample.");
            }

            bool replace = _lastRecordedWeek.HasValue && week == _lastRecordedWeek.Value;
            foreach (StatId id in _tracked)
            {
                List<float> list = _samples[id];
                float value = stats.GetValue(id);
                if (replace)
                {
                    list[list.Count - 1] = value;
                    continue;
                }

                list.Add(value);
                if (list.Count > Capacity)
                {
                    list.RemoveAt(0);
                }
            }
            _lastRecordedWeek = week;
        }

        /// <summary>
        /// Returns the samples of a tracked stat, oldest first.
        /// </summary>
        /// <param name="id">Tracked stat id.</param>
        /// <returns>Samples.</returns>
        public IReadOnlyList<float> GetSamples(StatId id)
        {
            if (!_samples.TryGetValue(id, out List<float> list))
            {
                throw new KeyNotFoundException($"Stat '{id}' is not tracked.");
            }
            return list;
        }

        /// <summary>
        /// Replaces all samples, used when loading a save. Extra samples beyond capacity keep the newest.
        /// </summary>
        /// <param name="id">Tracked stat id.</param>
        /// <param name="samples">Samples, oldest first.</param>
        /// <param name="lastRecordedWeek">Week of the newest sample, or null when empty.</param>
        public void Restore(StatId id, IEnumerable<float> samples, GameDate? lastRecordedWeek)
        {
            if (samples == null)
            {
                throw new ArgumentNullException(nameof(samples));
            }
            if (!_samples.TryGetValue(id, out List<float> list))
            {
                throw new KeyNotFoundException($"Stat '{id}' is not tracked.");
            }

            list.Clear();
            list.AddRange(samples);
            if (list.Count > Capacity)
            {
                list.RemoveRange(0, list.Count - Capacity);
            }
            _lastRecordedWeek = lastRecordedWeek;
        }
    }
}
