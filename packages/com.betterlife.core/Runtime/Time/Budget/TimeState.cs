using System;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Time.Budget
{
    /// <summary>
    /// The saved time state of a life: the clock and the player's weekly plan.
    /// </summary>
    public sealed class TimeState
    {
        /// <summary>
        /// Creates a time state.
        /// </summary>
        /// <param name="clock">Game clock.</param>
        /// <param name="budget">Weekly plan.</param>
        public TimeState(GameClock clock, TimeBudget budget)
        {
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Budget = budget ?? throw new ArgumentNullException(nameof(budget));
        }

        /// <summary>Game clock.</summary>
        public GameClock Clock { get; }

        /// <summary>Weekly plan.</summary>
        public TimeBudget Budget { get; }
    }
}
