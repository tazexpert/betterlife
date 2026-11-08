using System;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Monthly career review, before the finance month pays the salary.
    /// </summary>
    public sealed class CareerMonthPhase : ITickPhase
    {
        private const int OrderAfterDecay = 40;

        private readonly CareerService _career;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="career">Career service.</param>
        public CareerMonthPhase(CareerService career)
        {
            _career = career ?? throw new ArgumentNullException(nameof(career));
        }

        /// <inheritdoc />
        public string Name => "career_month";

        /// <inheritdoc />
        public int Order => TickOrder.NaturalDecay + OrderAfterDecay;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Monthly;

        /// <inheritdoc />
        public void Execute(TickContext context) => _career.ProcessMonth(context);
    }
}
