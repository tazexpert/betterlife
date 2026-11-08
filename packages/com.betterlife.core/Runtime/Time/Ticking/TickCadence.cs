namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// How often a tick phase runs.
    /// </summary>
    public enum TickCadence
    {
        /// <summary>Every week (micro).</summary>
        Weekly = 0,
        /// <summary>In the first week of each month (finance).</summary>
        Monthly = 1,
        /// <summary>In the first week of each year (macro).</summary>
        Yearly = 2,
    }
}
