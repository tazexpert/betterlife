using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// The Work tab: school and job now, and where the character can apply.
    /// </summary>
    public sealed class WorkState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="education">Current enrollment and degrees.</param>
        /// <param name="canDropOut">Whether the character is enrolled and may leave.</param>
        /// <param name="educationOptions">Schools open to the character now.</param>
        /// <param name="job">Current job (or not working).</param>
        /// <param name="canQuitJob">Whether the character has a job to quit.</param>
        /// <param name="jobOptions">Careers open to the character now.</param>
        public WorkState(IReadOnlyList<ValueRow> education, bool canDropOut, IReadOnlyList<EducationOption> educationOptions,
            IReadOnlyList<ValueRow> job, bool canQuitJob, IReadOnlyList<JobOption> jobOptions)
        {
            Education = education;
            CanDropOut = canDropOut;
            EducationOptions = educationOptions;
            Job = job;
            CanQuitJob = canQuitJob;
            JobOptions = jobOptions;
        }

        /// <summary>Current enrollment and degrees.</summary>
        public IReadOnlyList<ValueRow> Education { get; }

        /// <summary>Whether the character may leave school.</summary>
        public bool CanDropOut { get; }

        /// <summary>Schools open to the character now.</summary>
        public IReadOnlyList<EducationOption> EducationOptions { get; }

        /// <summary>Current job.</summary>
        public IReadOnlyList<ValueRow> Job { get; }

        /// <summary>Whether the character has a job to quit.</summary>
        public bool CanQuitJob { get; }

        /// <summary>Careers open to the character now.</summary>
        public IReadOnlyList<JobOption> JobOptions { get; }
    }
}
