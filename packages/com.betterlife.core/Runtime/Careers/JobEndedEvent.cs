using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// The character's job ended (quit, fired, laid off, risk or retirement).
    /// </summary>
    public sealed class JobEndedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="job">The ended job.</param>
        /// <param name="week">Week.</param>
        public JobEndedEvent(PastJob job, GameDate week)
        {
            Job = job;
            Week = week;
        }

        /// <summary>The ended job.</summary>
        public PastJob Job { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
