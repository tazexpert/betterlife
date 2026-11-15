using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// A job the character used to have.
    /// </summary>
    public sealed class PastJob
    {
        /// <summary>
        /// Creates the record.
        /// </summary>
        /// <param name="trackId">Career track id.</param>
        /// <param name="levelIndex">Highest level reached.</param>
        /// <param name="startedOn">First week.</param>
        /// <param name="endedOn">Last week.</param>
        /// <param name="reason">Why it ended.</param>
        public PastJob(string trackId, int levelIndex, GameDate startedOn, GameDate endedOn, JobEndReason reason)
        {
            if (levelIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(levelIndex), levelIndex, "Level must be non-negative.");
            }
            TrackId = ContentId.Require(trackId, nameof(trackId));
            LevelIndex = levelIndex;
            StartedOn = startedOn;
            EndedOn = endedOn;
            Reason = reason;
        }

        /// <summary>Career track id.</summary>
        public string TrackId { get; }

        /// <summary>Highest level reached.</summary>
        public int LevelIndex { get; }

        /// <summary>First week.</summary>
        public GameDate StartedOn { get; }

        /// <summary>Last week.</summary>
        public GameDate EndedOn { get; }

        /// <summary>Why it ended.</summary>
        public JobEndReason Reason { get; }
    }
}
