using System;

namespace BetterLife.Core.Saving
{
    /// <summary>
    /// The device's local date.
    /// </summary>
    public sealed class SystemRealTimeClock : IRealTimeClock
    {
        /// <inheritdoc />
        public DateTime Today => DateTime.Now.Date;
    }
}
