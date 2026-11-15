using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a past job.</summary>
    [Serializable]
    public sealed class PastJobDto
    {
        /// <summary>Career track id.</summary>
        public string TrackId;

        /// <summary>Highest level reached.</summary>
        public int LevelIndex;

        /// <summary>First week (GameDate.TotalWeeks).</summary>
        public int StartedWeek;

        /// <summary>Last week (GameDate.TotalWeeks).</summary>
        public int EndedWeek;

        /// <summary>Why it ended (enum name).</summary>
        public string Reason;
    }
}
