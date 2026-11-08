namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Whether an event is good or bad news (used by anti-frustration).
    /// </summary>
    public enum EventTone
    {
        /// <summary>Bad news.</summary>
        Negative = 0,
        /// <summary>Neither.</summary>
        Neutral = 1,
        /// <summary>Good news.</summary>
        Positive = 2,
    }
}
