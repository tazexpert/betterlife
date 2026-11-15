namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// A school or university the character can apply to now.
    /// </summary>
    public sealed class EducationOption
    {
        /// <summary>
        /// Creates an option.
        /// </summary>
        /// <param name="programId">Program id.</param>
        /// <param name="institutionId">Institution id.</param>
        /// <param name="funding">Who would pay the tuition.</param>
        /// <param name="title">Program and institution.</param>
        /// <param name="detail">Admission chance, tuition, scholarship, who pays.</param>
        public EducationOption(string programId, string institutionId, Education.TuitionFunding funding, string title, string detail)
        {
            ProgramId = programId;
            InstitutionId = institutionId;
            Funding = funding;
            Title = title;
            Detail = detail;
        }

        /// <summary>Program id.</summary>
        public string ProgramId { get; }

        /// <summary>Institution id.</summary>
        public string InstitutionId { get; }

        /// <summary>Who would pay the tuition.</summary>
        public Education.TuitionFunding Funding { get; }

        /// <summary>Program and institution.</summary>
        public string Title { get; }

        /// <summary>Admission chance, tuition, scholarship, who pays.</summary>
        public string Detail { get; }
    }
}
