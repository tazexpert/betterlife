using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// A school year ended: the student passed or must repeat it.
    /// </summary>
    public sealed class SchoolYearEndedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="enrollment">The enrollment.</param>
        /// <param name="passed">True when the year was passed.</param>
        /// <param name="performance">Performance at year end.</param>
        /// <param name="week">Week.</param>
        public SchoolYearEndedEvent(Enrollment enrollment, bool passed, float performance, GameDate week)
        {
            Enrollment = enrollment;
            Passed = passed;
            Performance = performance;
            Week = week;
        }

        /// <summary>The enrollment.</summary>
        public Enrollment Enrollment { get; }

        /// <summary>True when passed.</summary>
        public bool Passed { get; }

        /// <summary>Performance at year end.</summary>
        public float Performance { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
