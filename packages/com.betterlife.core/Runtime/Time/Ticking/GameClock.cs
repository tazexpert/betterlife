namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// The current in-game week. Only the time system moves it forward.
    /// </summary>
    public sealed class GameClock
    {
        /// <summary>
        /// Creates a clock.
        /// </summary>
        /// <param name="now">The current week (already simulated).</param>
        public GameClock(GameDate now)
        {
            Now = now;
        }

        /// <summary>The current week (already simulated).</summary>
        public GameDate Now { get; private set; }

        internal GameDate NextWeek => Now.AddWeeks(1);

        internal void MoveTo(GameDate week)
        {
            Now = week;
        }
    }
}
