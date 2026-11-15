using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// The character started a job.
    /// </summary>
    public sealed class JobStartedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="job">The job.</param>
        /// <param name="week">Week.</param>
        public JobStartedEvent(Job job, GameDate week)
        {
            Job = job;
            Week = week;
        }

        /// <summary>The job.</summary>
        public Job Job { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
