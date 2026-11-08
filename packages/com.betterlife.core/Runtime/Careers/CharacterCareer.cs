using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// A character's working life: the current job, past jobs, retirement and pension, and recent applications.
    /// </summary>
    public sealed class CharacterCareer
    {
        private readonly List<PastJob> _history = new List<PastJob>();
        private readonly Dictionary<string, GameDate> _attempts = new Dictionary<string, GameDate>(StringComparer.Ordinal);

        /// <summary>Current job, or null.</summary>
        public Job Current { get; private set; }

        /// <summary>True while working.</summary>
        public bool IsEmployed => Current != null;

        /// <summary>Past jobs, oldest first.</summary>
        public IReadOnlyList<PastJob> History => _history;

        /// <summary>True once retired.</summary>
        public bool IsRetired { get; private set; }

        /// <summary>Real monthly pension (0 when none).</summary>
        public decimal PensionMonthly { get; private set; }

        /// <summary>Last application week per track.</summary>
        public IReadOnlyDictionary<string, GameDate> Attempts => _attempts;

        /// <summary>
        /// Starts a job.
        /// </summary>
        /// <param name="job">The job.</param>
        public void Start(Job job)
        {
            if (Current != null)
            {
                throw new InvalidOperationException("Already employed.");
            }
            Current = job ?? throw new ArgumentNullException(nameof(job));
        }

        /// <summary>
        /// Ends the current job and records it.
        /// </summary>
        /// <param name="reason">Why it ends.</param>
        /// <param name="week">Last week.</param>
        /// <returns>The record, or null when not employed.</returns>
        public PastJob End(JobEndReason reason, GameDate week)
        {
            if (Current == null)
            {
                return null;
            }
            var past = new PastJob(Current.TrackId, Current.LevelIndex, Current.StartedOn, week, reason);
            _history.Add(past);
            Current = null;
            return past;
        }

        /// <summary>
        /// Marks the character as retired with a pension.
        /// </summary>
        /// <param name="pensionMonthly">Real monthly pension; non-negative.</param>
        public void Retire(decimal pensionMonthly)
        {
            if (pensionMonthly < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(pensionMonthly), pensionMonthly, "Pension must be non-negative.");
            }
            IsRetired = true;
            PensionMonthly = pensionMonthly;
        }

        /// <summary>
        /// Records a job application.
        /// </summary>
        /// <param name="trackId">Track id.</param>
        /// <param name="week">Week.</param>
        public void RecordAttempt(string trackId, GameDate week) => _attempts[ContentId.Require(trackId, nameof(trackId))] = week;

        /// <summary>
        /// Restores saved state.
        /// </summary>
        /// <param name="current">Job, or null.</param>
        /// <param name="history">Past jobs.</param>
        /// <param name="retired">Retired.</param>
        /// <param name="pensionMonthly">Pension.</param>
        /// <param name="attempts">Applications.</param>
        public void Restore(Job current, IEnumerable<PastJob> history, bool retired, decimal pensionMonthly, IEnumerable<KeyValuePair<string, GameDate>> attempts)
        {
            Current = current;
            _history.Clear();
            _history.AddRange(history ?? Enumerable.Empty<PastJob>());
            IsRetired = retired;
            PensionMonthly = Math.Max(0m, pensionMonthly);
            _attempts.Clear();
            foreach (KeyValuePair<string, GameDate> attempt in attempts ?? Enumerable.Empty<KeyValuePair<string, GameDate>>())
            {
                _attempts[attempt.Key] = attempt.Value;
            }
        }

        /// <summary>
        /// Removes tracks that no longer exist in content; returns one warning per removal.
        /// </summary>
        /// <param name="levelCount">Number of levels of a known track, or null for unknown tracks.</param>
        /// <returns>Warnings.</returns>
        public IReadOnlyList<string> DropUnknown(Func<string, int?> levelCount)
        {
            if (levelCount == null)
            {
                throw new ArgumentNullException(nameof(levelCount));
            }
            var warnings = new List<string>();
            if (Current != null)
            {
                int? levels = levelCount(Current.TrackId);
                if (!levels.HasValue || Current.LevelIndex >= levels.Value)
                {
                    warnings.Add($"Unknown career '{Current.TrackId}' job removed.");
                    Current = null;
                }
            }
            foreach (PastJob job in _history.Where(j => !levelCount(j.TrackId).HasValue).ToList())
            {
                _history.Remove(job);
                warnings.Add($"Unknown career '{job.TrackId}' history entry removed.");
            }
            foreach (string id in _attempts.Keys.Where(id => !levelCount(id).HasValue).ToList())
            {
                _attempts.Remove(id);
            }
            return warnings;
        }
    }
}
