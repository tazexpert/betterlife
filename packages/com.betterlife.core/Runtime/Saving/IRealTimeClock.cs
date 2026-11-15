using System;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// The real-world calendar (for daily limits), separate from game time.
    /// </summary>
    public interface IRealTimeClock
    {
        /// <summary>Today's date on the player's device.</summary>
        DateTime Today { get; }
    }
}
