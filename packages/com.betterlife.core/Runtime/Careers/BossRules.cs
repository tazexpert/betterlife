using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// How the boss reacts to performance, and when they fire.
    /// </summary>
    public sealed class BossRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="weeklySmoothing">Share of the gap to performance closed each week, (0, 1].</param>
        /// <param name="start">Satisfaction at hiring.</param>
        /// <param name="fireBelow">Satisfaction below which months count towards firing.</param>
        /// <param name="fireAfterMonths">Consecutive low months before firing.</param>
        /// <param name="stressFromDissatisfaction">Extra stress multiplier at zero satisfaction (0 at 50 or above).</param>
        public BossRules(float weeklySmoothing, float start, float fireBelow, int fireAfterMonths, float stressFromDissatisfaction)
        {
            if (!(weeklySmoothing > 0f) || weeklySmoothing > 1f || !StatBounds.IsInRange(start) || !StatBounds.IsInRange(fireBelow)
                || fireAfterMonths < 1 || stressFromDissatisfaction < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(weeklySmoothing), "Boss values are out of range.");
            }
            WeeklySmoothing = weeklySmoothing;
            Start = start;
            FireBelow = fireBelow;
            FireAfterMonths = fireAfterMonths;
            StressFromDissatisfaction = stressFromDissatisfaction;
        }

        /// <summary>Share of the gap closed each week.</summary>
        public float WeeklySmoothing { get; }

        /// <summary>Satisfaction at hiring.</summary>
        public float Start { get; }

        /// <summary>Firing line.</summary>
        public float FireBelow { get; }

        /// <summary>Consecutive low months before firing.</summary>
        public int FireAfterMonths { get; }

        /// <summary>Extra stress multiplier at zero satisfaction.</summary>
        public float StressFromDissatisfaction { get; }

        /// <summary>
        /// Returns the job stress multiplier for a satisfaction level (1 when the boss is at least neutral).
        /// </summary>
        /// <param name="satisfaction">Satisfaction 0-100.</param>
        /// <returns>Multiplier ≥ 1.</returns>
        public float StressMultiplier(float satisfaction)
        {
            const float Neutral = 50f;
            return 1f + Math.Max(0f, (Neutral - satisfaction) / Neutral) * StressFromDissatisfaction;
        }
    }
}
