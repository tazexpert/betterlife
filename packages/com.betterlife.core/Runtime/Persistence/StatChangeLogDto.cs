using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a stat change log entry.</summary>
    [Serializable]
    public sealed class StatChangeLogDto
    {
        /// <summary>Week (GameDate.TotalWeeks).</summary>
        public int Week;

        /// <summary>Stat id.</summary>
        public string StatId;

        /// <summary>Applied delta.</summary>
        public float Delta;

        /// <summary>Source localization key.</summary>
        public string SourceKey;
    }
}
