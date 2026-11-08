namespace BetterLife.Core.Careers
{
    /// <summary>
    /// What a career condition checks.
    /// </summary>
    public enum CareerConditionMode
    {
        /// <summary>Has (or has no) job.</summary>
        Employed = 0,

        /// <summary>Works in a category.</summary>
        Category = 1,

        /// <summary>Works in a track, optionally at least at a level.</summary>
        Track = 2,
    }
}
