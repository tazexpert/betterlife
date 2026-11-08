namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// How far the player asks to skip.
    /// </summary>
    public enum TimeSkip
    {
        /// <summary>One week.</summary>
        Week = 0,
        /// <summary>Until the first week of the next month.</summary>
        Month = 1,
        /// <summary>Until the first week of the next year.</summary>
        Year = 2,
    }
}
