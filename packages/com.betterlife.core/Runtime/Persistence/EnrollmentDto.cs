using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of the current enrollment.</summary>
    [Serializable]
    public sealed class EnrollmentDto
    {
        /// <summary>Program id.</summary>
        public string ProgramId;

        /// <summary>Institution id.</summary>
        public string InstitutionId;

        /// <summary>First week (GameDate.TotalWeeks).</summary>
        public int StartedWeek;

        /// <summary>First week of the school year (GameDate.TotalWeeks).</summary>
        public int YearStartedWeek;

        /// <summary>School years passed.</summary>
        public int YearsCompleted;

        /// <summary>Years repeated.</summary>
        public int Repeats;

        /// <summary>Performance 0-100.</summary>
        public float Performance;

        /// <summary>On a scholarship.</summary>
        public bool Scholarship;

        /// <summary>Tuition funding (enum name).</summary>
        public string Funding;
    }
}
