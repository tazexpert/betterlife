using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// An enrollment ended (graduation, dropout or expulsion).
    /// </summary>
    public sealed class EducationEndedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="enrollment">The ended enrollment.</param>
        /// <param name="reason">Why it ended.</param>
        /// <param name="week">Week.</param>
        public EducationEndedEvent(Enrollment enrollment, EducationEndReason reason, GameDate week)
        {
            Enrollment = enrollment;
            Reason = reason;
            Week = week;
        }

        /// <summary>The ended enrollment.</summary>
        public Enrollment Enrollment { get; }

        /// <summary>Why it ended.</summary>
        public EducationEndReason Reason { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
