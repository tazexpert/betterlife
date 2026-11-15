namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Direction of a stat over the trend window.
    /// </summary>
    public enum TrendDirection
    {
        /// <summary>Changed less than the flat threshold.</summary>
        Flat = 0,
        /// <summary>Went up.</summary>
        Up = 1,
        /// <summary>Went down.</summary>
        Down = 2,
    }
}
