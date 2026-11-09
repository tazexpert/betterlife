namespace BetterLife.Core.Vices
{
    /// <summary>
    /// What a vice condition compares.
    /// </summary>
    public enum ViceMeasure
    {
        /// <summary>Addiction 0-100.</summary>
        Addiction = 0,

        /// <summary>Tolerance 0-100.</summary>
        Tolerance = 1,

        /// <summary>Number of uses.</summary>
        Uses = 2,

        /// <summary>Weeks since the last use (0 when never used).</summary>
        CleanWeeks = 3,
    }
}
