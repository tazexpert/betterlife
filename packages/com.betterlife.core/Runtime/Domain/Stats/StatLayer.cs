namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Visibility tier of a stat.
    /// </summary>
    public enum StatLayer
    {
        /// <summary>One of the five headline stats shown as dashboard cards.</summary>
        Core = 0,
        /// <summary>A detail stat that feeds a core stat.</summary>
        Sub = 1,
        /// <summary>A stat the player cannot see directly.</summary>
        Hidden = 2,
    }
}
