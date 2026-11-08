using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Save representation of a <see cref="BetterLife.Core.Time.CalendarDate"/>.
    /// </summary>
    [Serializable]
    public sealed class CalendarDateDto
    {
        /// <summary>Year (Common Era).</summary>
        public int Year;

        /// <summary>Month, 1 to 12.</summary>
        public int Month;

        /// <summary>Day of month.</summary>
        public int Day;
    }
}
