using System;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// Weekly schooling after the time budget has been applied.
    /// </summary>
    public sealed class EducationWeekPhase : ITickPhase
    {
        private const int OrderAfterTimeAllocation = 10;

        private readonly EducationService _education;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="education">Education service.</param>
        public EducationWeekPhase(EducationService education)
        {
            _education = education ?? throw new ArgumentNullException(nameof(education));
        }

        /// <inheritdoc />
        public string Name => "education_week";

        /// <inheritdoc />
        public int Order => TickOrder.TimeAllocation + OrderAfterTimeAllocation;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Weekly;

        /// <inheritdoc />
        public void Execute(TickContext context) => _education.ProcessWeek(context);
    }
}
