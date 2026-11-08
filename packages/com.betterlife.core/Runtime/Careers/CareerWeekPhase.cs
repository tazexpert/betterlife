using System;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Weekly work after the time budget has been applied.
    /// </summary>
    public sealed class CareerWeekPhase : ITickPhase
    {
        private const int OrderAfterTimeAllocation = 20;

        private readonly CareerService _career;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="career">Career service.</param>
        public CareerWeekPhase(CareerService career)
        {
            _career = career ?? throw new ArgumentNullException(nameof(career));
        }

        /// <inheritdoc />
        public string Name => "career_week";

        /// <inheritdoc />
        public int Order => TickOrder.TimeAllocation + OrderAfterTimeAllocation;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Weekly;

        /// <inheritdoc />
        public void Execute(TickContext context) => _career.ProcessWeek(context);
    }
}
