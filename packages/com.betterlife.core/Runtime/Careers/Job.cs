using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// The character's current job.
    /// </summary>
    public sealed class Job
    {
        /// <summary>
        /// Creates or restores a job.
        /// </summary>
        /// <param name="trackId">Career track id.</param>
        /// <param name="levelIndex">Level index on the ladder.</param>
        /// <param name="startedOn">First week in the track.</param>
        /// <param name="levelSince">First week in the current level.</param>
        /// <param name="performance">Performance 0-100.</param>
        /// <param name="bossSatisfaction">Boss satisfaction 0-100.</param>
        /// <param name="lowBossMonths">Consecutive months with boss satisfaction below the firing line.</param>
        /// <param name="nextReviewOn">Week of the next promotion review, or null.</param>
        public Job(string trackId, int levelIndex, GameDate startedOn, GameDate levelSince, float performance, float bossSatisfaction,
            int lowBossMonths, GameDate? nextReviewOn)
        {
            if (levelIndex < 0 || lowBossMonths < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(levelIndex), "Level and months must be non-negative.");
            }
            TrackId = ContentId.Require(trackId, nameof(trackId));
            LevelIndex = levelIndex;
            StartedOn = startedOn;
            LevelSince = levelSince;
            Performance = StatBounds.Clamp(performance);
            BossSatisfaction = StatBounds.Clamp(bossSatisfaction);
            LowBossMonths = lowBossMonths;
            NextReviewOn = nextReviewOn;
        }

        /// <summary>Career track id.</summary>
        public string TrackId { get; }

        /// <summary>Level index on the ladder.</summary>
        public int LevelIndex { get; private set; }

        /// <summary>First week in the track.</summary>
        public GameDate StartedOn { get; }

        /// <summary>First week in the current level.</summary>
        public GameDate LevelSince { get; private set; }

        /// <summary>Performance 0-100.</summary>
        public float Performance { get; private set; }

        /// <summary>Boss satisfaction 0-100.</summary>
        public float BossSatisfaction { get; private set; }

        /// <summary>Consecutive months below the firing line.</summary>
        public int LowBossMonths { get; private set; }

        /// <summary>Week of the next promotion review, or null.</summary>
        public GameDate? NextReviewOn { get; private set; }

        /// <summary>
        /// Changes performance, clamped to 0-100.
        /// </summary>
        /// <param name="delta">Change.</param>
        public void ChangePerformance(float delta) => Performance = StatBounds.Clamp(Performance + delta);

        /// <summary>
        /// Changes boss satisfaction, clamped to 0-100.
        /// </summary>
        /// <param name="delta">Change.</param>
        public void ChangeBossSatisfaction(float delta) => BossSatisfaction = StatBounds.Clamp(BossSatisfaction + delta);

        internal void Promote(GameDate week, GameDate? nextReview)
        {
            LevelIndex++;
            LevelSince = week;
            NextReviewOn = nextReview;
        }

        internal void ScheduleReview(GameDate? week) => NextReviewOn = week;

        internal void CountBossMonth(bool low) => LowBossMonths = low ? LowBossMonths + 1 : 0;
    }
}
