using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// The program the character is studying in now.
    /// </summary>
    public sealed class Enrollment
    {
        /// <summary>
        /// Creates or restores an enrollment.
        /// </summary>
        /// <param name="programId">Program id.</param>
        /// <param name="institutionId">Institution id.</param>
        /// <param name="startedOn">First week.</param>
        /// <param name="yearStartedOn">First week of the current school year.</param>
        /// <param name="yearsCompleted">School years passed.</param>
        /// <param name="repeats">Years repeated so far.</param>
        /// <param name="performance">Performance 0-100.</param>
        /// <param name="scholarship">True while on a scholarship.</param>
        /// <param name="funding">Who pays the tuition.</param>
        public Enrollment(string programId, string institutionId, GameDate startedOn, GameDate yearStartedOn, int yearsCompleted, int repeats,
            float performance, bool scholarship, TuitionFunding funding)
        {
            if (yearsCompleted < 0 || repeats < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(yearsCompleted), "Counts must be non-negative.");
            }
            ProgramId = ContentId.Require(programId, nameof(programId));
            InstitutionId = ContentId.Require(institutionId, nameof(institutionId));
            StartedOn = startedOn;
            YearStartedOn = yearStartedOn;
            YearsCompleted = yearsCompleted;
            Repeats = repeats;
            Performance = StatBounds.Clamp(performance);
            Scholarship = scholarship;
            Funding = funding;
        }

        /// <summary>Program id.</summary>
        public string ProgramId { get; }

        /// <summary>Institution id.</summary>
        public string InstitutionId { get; }

        /// <summary>First week.</summary>
        public GameDate StartedOn { get; }

        /// <summary>First week of the current school year.</summary>
        public GameDate YearStartedOn { get; private set; }

        /// <summary>School years passed.</summary>
        public int YearsCompleted { get; private set; }

        /// <summary>Years repeated so far.</summary>
        public int Repeats { get; private set; }

        /// <summary>Performance 0-100.</summary>
        public float Performance { get; private set; }

        /// <summary>Grade point average 0-4 shown to the player.</summary>
        public float Gpa => Credential.ToGpa(Performance);

        /// <summary>True while on a scholarship.</summary>
        public bool Scholarship { get; private set; }

        /// <summary>Who pays the tuition.</summary>
        public TuitionFunding Funding { get; private set; }

        /// <summary>
        /// Changes performance, clamped to 0-100.
        /// </summary>
        /// <param name="delta">Change.</param>
        public void ChangePerformance(float delta) => Performance = StatBounds.Clamp(Performance + delta);

        internal void PassYear(GameDate nextYearStart)
        {
            YearsCompleted++;
            YearStartedOn = nextYearStart;
        }

        internal void RepeatYear(GameDate nextYearStart)
        {
            Repeats++;
            YearStartedOn = nextYearStart;
        }

        internal void SetScholarship(bool scholarship) => Scholarship = scholarship;

        internal void SetFunding(TuitionFunding funding) => Funding = funding;
    }
}
