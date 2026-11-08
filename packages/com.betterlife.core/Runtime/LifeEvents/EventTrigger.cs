namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// How an event can occur.
    /// </summary>
    public enum EventTrigger
    {
        /// <summary>Drawn from the weekly random pool.</summary>
        Random = 0,
        /// <summary>Only occurs when scheduled by an earlier choice or event.</summary>
        Scheduled = 1,
        /// <summary>Drawn when a karmic debt comes due.</summary>
        KarmicDebt = 2,
    }
}
