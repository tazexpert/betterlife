using System;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Luck;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// Scholarship rules: a strong exam score may win free tuition, kept while performance stays high.
    /// </summary>
    public sealed class ScholarshipRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="minScore">Lowest exam score that may win a scholarship.</param>
        /// <param name="chance">Chance to win when eligible.</param>
        /// <param name="keepPerformance">Yearly performance needed to keep it.</param>
        public ScholarshipRules(float minScore, ChanceSpec chance, float keepPerformance)
        {
            if (!StatBounds.IsInRange(minScore) || !StatBounds.IsInRange(keepPerformance))
            {
                throw new ArgumentOutOfRangeException(nameof(minScore), "Scores must be within 0-100.");
            }
            MinScore = minScore;
            Chance = chance ?? throw new ArgumentNullException(nameof(chance));
            KeepPerformance = keepPerformance;
        }

        /// <summary>Lowest exam score that may win a scholarship.</summary>
        public float MinScore { get; }

        /// <summary>Chance to win when eligible.</summary>
        public ChanceSpec Chance { get; }

        /// <summary>Yearly performance needed to keep it.</summary>
        public float KeepPerformance { get; }
    }
}
