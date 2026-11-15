namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// How a row reads for the character: good news, bad news or neutral.
    /// </summary>
    public enum RowTone
    {
        /// <summary>Neither good nor bad.</summary>
        Neutral = 0,
        /// <summary>Good for the character.</summary>
        Good = 1,
        /// <summary>Bad for the character.</summary>
        Bad = 2,
    }
}
