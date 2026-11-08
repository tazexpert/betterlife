using System;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Luck;

namespace BetterLife.Core.Education
{
    /// <summary>
    /// Entrance exam rules.
    /// </summary>
    public sealed class AdmissionRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="chance">Base chance spec (stats, fortune, karma).</param>
        /// <param name="perPointAboveDifficulty">Chance points per exam score point above the difficulty.</param>
        /// <param name="performanceWeight">Weight of the latest credential's performance in the exam score.</param>
        /// <param name="knowledgeWeight">Weight of Knowledge in the exam score.</param>
        /// <param name="retryWeeks">Weeks before the same program can be tried again.</param>
        public AdmissionRules(ChanceSpec chance, float perPointAboveDifficulty, float performanceWeight, float knowledgeWeight, int retryWeeks)
        {
            if (perPointAboveDifficulty < 0f || performanceWeight < 0f || knowledgeWeight < 0f || performanceWeight + knowledgeWeight <= 0f || retryWeeks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(perPointAboveDifficulty), "Exam values are out of range.");
            }
            Chance = chance ?? throw new ArgumentNullException(nameof(chance));
            PerPointAboveDifficulty = perPointAboveDifficulty;
            PerformanceWeight = performanceWeight;
            KnowledgeWeight = knowledgeWeight;
            RetryWeeks = retryWeeks;
        }

        /// <summary>Base chance spec.</summary>
        public ChanceSpec Chance { get; }

        /// <summary>Chance points per score point above the difficulty.</summary>
        public float PerPointAboveDifficulty { get; }

        /// <summary>Weight of the latest credential's performance.</summary>
        public float PerformanceWeight { get; }

        /// <summary>Weight of Knowledge.</summary>
        public float KnowledgeWeight { get; }

        /// <summary>Weeks before the same program can be tried again.</summary>
        public int RetryWeeks { get; }

        /// <summary>
        /// Returns the exam score 0-100.
        /// </summary>
        /// <param name="credentialPerformance">Performance of the latest credential (or current studies).</param>
        /// <param name="knowledge">Knowledge.</param>
        /// <returns>Score.</returns>
        public float Score(float credentialPerformance, float knowledge)
        {
            return StatBounds.Clamp((credentialPerformance * PerformanceWeight + knowledge * KnowledgeWeight) / (PerformanceWeight + KnowledgeWeight));
        }
    }
}
