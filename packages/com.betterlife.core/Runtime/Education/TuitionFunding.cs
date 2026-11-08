namespace BetterLife.Core.Education
{
    /// <summary>
    /// Who pays the yearly tuition.
    /// </summary>
    public enum TuitionFunding
    {
        /// <summary>The family pays (dependents only).</summary>
        Family = 0,

        /// <summary>The character pays from cash; shortfalls become debt.</summary>
        Cash = 1,

        /// <summary>A student loan pays each year.</summary>
        StudentLoan = 2,
    }
}
