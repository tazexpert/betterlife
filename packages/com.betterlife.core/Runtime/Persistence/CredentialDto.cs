using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a completed program.</summary>
    [Serializable]
    public sealed class CredentialDto
    {
        /// <summary>Program id.</summary>
        public string ProgramId;

        /// <summary>Institution id.</summary>
        public string InstitutionId;

        /// <summary>Graduation week (GameDate.TotalWeeks).</summary>
        public int GraduatedWeek;

        /// <summary>Performance at graduation.</summary>
        public float FinalPerformance;
    }
}
