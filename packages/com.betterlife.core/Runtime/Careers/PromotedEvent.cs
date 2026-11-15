using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// The character moved up the career ladder.
    /// </summary>
    public sealed class PromotedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="job">The job after the promotion.</param>
        /// <param name="week">Week.</param>
        public PromotedEvent(Job job, GameDate week)
        {
            Job = job;
            Week = week;
        }

        /// <summary>The job after the promotion.</summary>
        public Job Job { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
