using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// A character started a program.
    /// </summary>
    public sealed class EnrolledEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="enrollment">The enrollment.</param>
        /// <param name="week">Week.</param>
        public EnrolledEvent(Enrollment enrollment, GameDate week)
        {
            Enrollment = enrollment;
            Week = week;
        }

        /// <summary>The enrollment.</summary>
        public Enrollment Enrollment { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
