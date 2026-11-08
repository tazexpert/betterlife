namespace BetterLife.Core.Time
{
    /// <summary>
    /// Thai astrological birth day. Wednesday is split into day and night (Rahu).
    /// </summary>
    public enum ThaiDayOfWeek
    {
        /// <summary>Sunday.</summary>
        Sunday = 0,
        /// <summary>Monday.</summary>
        Monday = 1,
        /// <summary>Tuesday.</summary>
        Tuesday = 2,
        /// <summary>Wednesday daytime (06:00-17:59).</summary>
        Wednesday = 3,
        /// <summary>Thursday.</summary>
        Thursday = 4,
        /// <summary>Friday.</summary>
        Friday = 5,
        /// <summary>Saturday.</summary>
        Saturday = 6,
        /// <summary>Wednesday night, ruled by Rahu (Wednesday 18:00 until Thursday 05:59).</summary>
        WednesdayNight = 7,
    }
}
