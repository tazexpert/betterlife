using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// One applied stat change kept for trend breakdowns.
    /// </summary>
    public sealed class StatChangeLogEntry
    {
        /// <summary>
        /// Creates an entry.
        /// </summary>
        /// <param name="week">Week the change happened.</param>
        /// <param name="statId">Changed stat.</param>
        /// <param name="delta">Applied delta.</param>
        /// <param name="sourceKey">Localization key of the cause.</param>
        public StatChangeLogEntry(GameDate week, StatId statId, float delta, string sourceKey)
        {
            Week = week;
            StatId = statId;
            Delta = delta;
            SourceKey = sourceKey;
        }

        /// <summary>Week the change happened.</summary>
        public GameDate Week { get; }

        /// <summary>Changed stat.</summary>
        public StatId StatId { get; }

        /// <summary>Applied delta.</summary>
        public float Delta { get; }

        /// <summary>Localization key of the cause.</summary>
        public string SourceKey { get; }
    }
}
