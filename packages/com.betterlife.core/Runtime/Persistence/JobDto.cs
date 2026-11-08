using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of the current job.</summary>
    [Serializable]
    public sealed class JobDto
    {
        /// <summary>Career track id.</summary>
        public string TrackId;

        /// <summary>Level index.</summary>
        public int LevelIndex;

        /// <summary>First week in the track (GameDate.TotalWeeks).</summary>
        public int StartedWeek;

        /// <summary>First week in the level (GameDate.TotalWeeks).</summary>
        public int LevelSinceWeek;

        /// <summary>Performance 0-100.</summary>
        public float Performance;

        /// <summary>Boss satisfaction 0-100.</summary>
        public float BossSatisfaction;

        /// <summary>Consecutive low-satisfaction months.</summary>
        public int LowBossMonths;

        /// <summary>Whether NextReviewWeek is meaningful.</summary>
        public bool HasReview;

        /// <summary>Next promotion review (GameDate.TotalWeeks) when HasReview.</summary>
        public int NextReviewWeek;
    }
}
