using System;

namespace BetterLife.Core.Domain.Skills
{
    /// <summary>
    /// How a skill fades when unused.
    /// </summary>
    public sealed class SkillDecayRule
    {
        /// <summary>
        /// Creates a decay rule.
        /// </summary>
        /// <param name="graceWeeks">Idle weeks before decay starts; non-negative.</param>
        /// <param name="ratePerWeek">Level lost per idle week after the grace period; non-negative.</param>
        /// <param name="retentionOfPeak">Fraction of the peak level that is never lost, 0 to 1.</param>
        public SkillDecayRule(int graceWeeks, float ratePerWeek, float retentionOfPeak)
        {
            if (graceWeeks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(graceWeeks), graceWeeks, "Grace weeks must be non-negative.");
            }
            if (float.IsNaN(ratePerWeek) || ratePerWeek < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(ratePerWeek), ratePerWeek, "Rate must be non-negative.");
            }
            if (float.IsNaN(retentionOfPeak) || retentionOfPeak < 0f || retentionOfPeak > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(retentionOfPeak), retentionOfPeak, "Retention must be in [0, 1].");
            }

            GraceWeeks = graceWeeks;
            RatePerWeek = ratePerWeek;
            RetentionOfPeak = retentionOfPeak;
        }

        /// <summary>Idle weeks before decay starts.</summary>
        public int GraceWeeks { get; }

        /// <summary>Level lost per idle week after the grace period.</summary>
        public float RatePerWeek { get; }

        /// <summary>Fraction of the peak level that is never lost.</summary>
        public float RetentionOfPeak { get; }
    }
}
