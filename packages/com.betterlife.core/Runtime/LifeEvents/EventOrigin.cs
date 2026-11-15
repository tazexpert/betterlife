namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Why a particular occurrence of an event happened.
    /// </summary>
    public enum EventOrigin
    {
        /// <summary>Weekly random roll.</summary>
        Random = 0,
        /// <summary>Scheduled earlier.</summary>
        Scheduled = 1,
        /// <summary>A karmic debt came due.</summary>
        KarmicDebt = 2,
    }
}
