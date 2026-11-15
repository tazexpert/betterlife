using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// Global education rules from content/education_settings.json.
    /// </summary>
    public sealed class EducationSettings
    {
        /// <summary>
        /// Creates the settings.
        /// </summary>
        /// <param name="schoolActivityId">Activity whose hours count as school attendance.</param>
        /// <param name="studyActivityId">Activity whose hours count as self-study.</param>
        /// <param name="startPerformance">Performance at enrollment.</param>
        /// <param name="performance">Weekly performance weights.</param>
        /// <param name="passPerformance">Yearly performance needed to pass.</param>
        /// <param name="maxRepeats">Repeated years allowed before expulsion.</param>
        /// <param name="admission">Entrance exam rules.</param>
        /// <param name="scholarship">Scholarship rules.</param>
        /// <param name="studentLoanProductId">Loan product used for student loans.</param>
        public EducationSettings(string schoolActivityId, string studyActivityId, float startPerformance, EducationPerformanceWeights performance,
            float passPerformance, int maxRepeats, AdmissionRules admission, ScholarshipRules scholarship, string studentLoanProductId)
        {
            if (!StatBounds.IsInRange(startPerformance) || !StatBounds.IsInRange(passPerformance) || maxRepeats < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(startPerformance), "Performance values must be within 0-100 and repeats non-negative.");
            }
            SchoolActivityId = ContentId.Require(schoolActivityId, nameof(schoolActivityId));
            StudyActivityId = ContentId.Require(studyActivityId, nameof(studyActivityId));
            StartPerformance = startPerformance;
            Performance = performance ?? throw new ArgumentNullException(nameof(performance));
            PassPerformance = passPerformance;
            MaxRepeats = maxRepeats;
            Admission = admission ?? throw new ArgumentNullException(nameof(admission));
            Scholarship = scholarship ?? throw new ArgumentNullException(nameof(scholarship));
            StudentLoanProductId = ContentId.Require(studentLoanProductId, nameof(studentLoanProductId));
        }

        /// <summary>Activity counted as school attendance.</summary>
        public string SchoolActivityId { get; }

        /// <summary>Activity counted as self-study.</summary>
        public string StudyActivityId { get; }

        /// <summary>Performance at enrollment.</summary>
        public float StartPerformance { get; }

        /// <summary>Weekly performance weights.</summary>
        public EducationPerformanceWeights Performance { get; }

        /// <summary>Yearly performance needed to pass.</summary>
        public float PassPerformance { get; }

        /// <summary>Repeated years allowed before expulsion.</summary>
        public int MaxRepeats { get; }

        /// <summary>Entrance exam rules.</summary>
        public AdmissionRules Admission { get; }

        /// <summary>Scholarship rules.</summary>
        public ScholarshipRules Scholarship { get; }

        /// <summary>Loan product used for student loans.</summary>
        public string StudentLoanProductId { get; }
    }
}
