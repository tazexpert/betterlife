using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// How weekly work performance is computed: a weighted target that performance moves towards.
    /// </summary>
    public sealed class WorkPerformanceWeights
    {
        private const float WeightTolerance = 0.001f;

        /// <summary>
        /// Creates the weights; the four weights must sum to 1.
        /// </summary>
        /// <param name="weeklySmoothing">Share of the gap to the target closed each week, (0, 1].</param>
        /// <param name="attendance">Weight of attendance (work hours / required hours).</param>
        /// <param name="skill">Weight of the track's performance skill (Knowledge when it has none).</param>
        /// <param name="mental">Weight of Mental.</param>
        /// <param name="rest">Weight of being rested (100 − Fatigue).</param>
        /// <param name="start">Performance at hiring.</param>
        public WorkPerformanceWeights(float weeklySmoothing, float attendance, float skill, float mental, float rest, float start)
        {
            if (!(weeklySmoothing > 0f) || weeklySmoothing > 1f || !StatBounds.IsInRange(start))
            {
                throw new ArgumentOutOfRangeException(nameof(weeklySmoothing), "Smoothing must be within (0, 1] and start within 0-100.");
            }
            if (attendance < 0f || skill < 0f || mental < 0f || rest < 0f || Math.Abs(attendance + skill + mental + rest - 1f) > WeightTolerance)
            {
                throw new ArgumentException("Weights must be non-negative and sum to 1.", nameof(attendance));
            }
            WeeklySmoothing = weeklySmoothing;
            Attendance = attendance;
            Skill = skill;
            Mental = mental;
            Rest = rest;
            Start = start;
        }

        /// <summary>Share of the gap closed each week.</summary>
        public float WeeklySmoothing { get; }

        /// <summary>Weight of attendance.</summary>
        public float Attendance { get; }

        /// <summary>Weight of the performance skill.</summary>
        public float Skill { get; }

        /// <summary>Weight of Mental.</summary>
        public float Mental { get; }

        /// <summary>Weight of being rested.</summary>
        public float Rest { get; }

        /// <summary>Performance at hiring.</summary>
        public float Start { get; }

        /// <summary>
        /// Returns this week's target performance.
        /// </summary>
        /// <param name="attendance">Worked share 0-1.</param>
        /// <param name="skill">Skill level 0-100.</param>
        /// <param name="mental">Mental 0-100.</param>
        /// <param name="fatigue">Fatigue 0-100.</param>
        /// <returns>Target 0-100.</returns>
        public float Target(float attendance, float skill, float mental, float fatigue)
        {
            float clamped = Math.Max(0f, Math.Min(1f, attendance));
            return StatBounds.Clamp(clamped * StatBounds.Max * Attendance + skill * Skill + mental * Mental + (StatBounds.Max - fatigue) * Rest);
        }
    }
}
