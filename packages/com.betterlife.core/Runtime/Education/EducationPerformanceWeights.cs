using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// How weekly school performance is computed: a weighted target that performance moves towards.
    /// </summary>
    public sealed class EducationPerformanceWeights
    {
        private const float WeightTolerance = 0.001f;

        /// <summary>
        /// Creates the weights; the four weights must sum to 1.
        /// </summary>
        /// <param name="weeklySmoothing">Share of the gap to the target closed each week, (0, 1].</param>
        /// <param name="attendance">Weight of attendance (school hours / required hours).</param>
        /// <param name="iq">Weight of IQ.</param>
        /// <param name="knowledge">Weight of Knowledge.</param>
        /// <param name="mental">Weight of Mental.</param>
        /// <param name="studyBonusPerHour">Bonus points per self-study hour.</param>
        /// <param name="maxStudyBonus">Cap of the study bonus.</param>
        public EducationPerformanceWeights(float weeklySmoothing, float attendance, float iq, float knowledge, float mental, float studyBonusPerHour, float maxStudyBonus)
        {
            if (!(weeklySmoothing > 0f) || weeklySmoothing > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(weeklySmoothing), weeklySmoothing, "Smoothing must be within (0, 1].");
            }
            if (attendance < 0f || iq < 0f || knowledge < 0f || mental < 0f || Math.Abs(attendance + iq + knowledge + mental - 1f) > WeightTolerance)
            {
                throw new ArgumentException("Weights must be non-negative and sum to 1.", nameof(attendance));
            }
            if (studyBonusPerHour < 0f || maxStudyBonus < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(studyBonusPerHour), "Study bonus must be non-negative.");
            }
            WeeklySmoothing = weeklySmoothing;
            Attendance = attendance;
            Iq = iq;
            Knowledge = knowledge;
            Mental = mental;
            StudyBonusPerHour = studyBonusPerHour;
            MaxStudyBonus = maxStudyBonus;
        }

        /// <summary>Share of the gap closed each week.</summary>
        public float WeeklySmoothing { get; }

        /// <summary>Weight of attendance.</summary>
        public float Attendance { get; }

        /// <summary>Weight of IQ.</summary>
        public float Iq { get; }

        /// <summary>Weight of Knowledge.</summary>
        public float Knowledge { get; }

        /// <summary>Weight of Mental.</summary>
        public float Mental { get; }

        /// <summary>Bonus points per self-study hour.</summary>
        public float StudyBonusPerHour { get; }

        /// <summary>Cap of the study bonus.</summary>
        public float MaxStudyBonus { get; }

        /// <summary>
        /// Returns this week's target performance.
        /// </summary>
        /// <param name="attendance">Attended share 0-1.</param>
        /// <param name="iq">IQ 0-100.</param>
        /// <param name="knowledge">Knowledge 0-100.</param>
        /// <param name="mental">Mental 0-100.</param>
        /// <param name="studyHours">Self-study hours this week.</param>
        /// <param name="quality">Institution quality bonus.</param>
        /// <returns>Target 0-100.</returns>
        public float Target(float attendance, float iq, float knowledge, float mental, int studyHours, float quality)
        {
            float clampedAttendance = Math.Max(0f, Math.Min(1f, attendance));
            float bonus = Math.Min(MaxStudyBonus, Math.Max(0, studyHours) * StudyBonusPerHour);
            return StatBounds.Clamp(clampedAttendance * StatBounds.Max * Attendance + iq * Iq + knowledge * Knowledge + mental * Mental + bonus + quality);
        }
    }
}
