namespace BetterLife.Core.Education
{
    /// <summary>
    /// Why an enrollment ended.
    /// </summary>
    public enum EducationEndReason
    {
        /// <summary>All years passed.</summary>
        Graduated = 0,

        /// <summary>The student left.</summary>
        DroppedOut = 1,

        /// <summary>Too many repeated years.</summary>
        Expelled = 2,
    }
}
