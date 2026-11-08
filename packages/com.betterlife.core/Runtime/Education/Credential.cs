using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// A completed program (diploma or degree).
    /// </summary>
    public sealed class Credential
    {
        private const float MaxGpa = 4f;

        /// <summary>
        /// Creates the credential.
        /// </summary>
        /// <param name="programId">Program id.</param>
        /// <param name="institutionId">Institution id.</param>
        /// <param name="graduatedOn">Graduation week.</param>
        /// <param name="finalPerformance">Performance at graduation, 0-100.</param>
        public Credential(string programId, string institutionId, GameDate graduatedOn, float finalPerformance)
        {
            ProgramId = ContentId.Require(programId, nameof(programId));
            InstitutionId = ContentId.Require(institutionId, nameof(institutionId));
            GraduatedOn = graduatedOn;
            FinalPerformance = StatBounds.Clamp(finalPerformance);
        }

        /// <summary>Program id.</summary>
        public string ProgramId { get; }

        /// <summary>Institution id.</summary>
        public string InstitutionId { get; }

        /// <summary>Graduation week.</summary>
        public GameDate GraduatedOn { get; }

        /// <summary>Performance at graduation.</summary>
        public float FinalPerformance { get; }

        /// <summary>Grade point average 0-4.</summary>
        public float Gpa => ToGpa(FinalPerformance);

        /// <summary>
        /// Converts performance 0-100 to a GPA 0-4.
        /// </summary>
        /// <param name="performance">Performance.</param>
        /// <returns>GPA.</returns>
        public static float ToGpa(float performance) => StatBounds.Clamp(performance) / StatBounds.Max * MaxGpa;
    }
}
