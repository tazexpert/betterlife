namespace BetterLife.Core.Education
{
    /// <summary>
    /// What an education condition checks.
    /// </summary>
    public enum EducationConditionMode
    {
        /// <summary>Holds a credential of at least the program's rank.</summary>
        Completed = 0,

        /// <summary>Is (or is not) studying anything.</summary>
        Enrolled = 1,

        /// <summary>Is studying a specific program.</summary>
        EnrolledIn = 2,
    }
}
