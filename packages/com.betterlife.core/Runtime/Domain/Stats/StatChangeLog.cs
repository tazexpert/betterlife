using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Rolling record of applied stat changes, used to explain why a stored stat is trending.
    /// </summary>
    public sealed class StatChangeLog
    {
        /// <summary>Default number of weeks kept.</summary>
        public const int DefaultRetentionWeeks = 4;

        private readonly List<StatChangeLogEntry> _entries = new List<StatChangeLogEntry>();

        /// <summary>
        /// Creates a log.
        /// </summary>
        /// <param name="retentionWeeks">Weeks to keep, including the current week; must be positive.</param>
        public StatChangeLog(int retentionWeeks = DefaultRetentionWeeks)
        {
            if (retentionWeeks <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(retentionWeeks), retentionWeeks, "Retention must be positive.");
            }
            RetentionWeeks = retentionWeeks;
        }

        /// <summary>Weeks kept, including the current week.</summary>
        public int RetentionWeeks { get; }

        /// <summary>All kept entries, oldest first.</summary>
        public IReadOnlyList<StatChangeLogEntry> Entries => _entries;

        /// <summary>
        /// Appends an entry and drops entries outside the retention window of its week.
        /// </summary>
        /// <param name="entry">Entry to add.</param>
        public void Add(StatChangeLogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }
            _entries.Add(entry);
            Prune(entry.Week);
        }

        /// <summary>
        /// Drops entries older than the retention window ending at <paramref name="now"/>.
        /// </summary>
        /// <param name="now">Current week.</param>
        public void Prune(GameDate now)
        {
            GameDate oldestKept = now.AddWeeks(1 - RetentionWeeks);
            _entries.RemoveAll(e => e.Week < oldestKept);
        }

        /// <summary>
        /// Sums the applied deltas of a stat per source within the retention window ending at <paramref name="now"/>.
        /// </summary>
        /// <param name="statId">Stat to summarize.</param>
        /// <param name="now">Current week.</param>
        /// <returns>One line per source, largest absolute change first.</returns>
        public IReadOnlyList<StatBreakdownLine> Summarize(StatId statId, GameDate now)
        {
            GameDate oldestKept = now.AddWeeks(1 - RetentionWeeks);
            return _entries
                .Where(e => e.StatId == statId && e.Week >= oldestKept && e.Week <= now)
                .GroupBy(e => e.SourceKey, StringComparer.Ordinal)
                .Select(g => new StatBreakdownLine(g.Key, g.Sum(e => e.Delta)))
                .OrderByDescending(line => Math.Abs(line.Contribution))
                .ThenBy(line => line.LabelKey, StringComparer.Ordinal)
                .ToList();
        }
    }
}
