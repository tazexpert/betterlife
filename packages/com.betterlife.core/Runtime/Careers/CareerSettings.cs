using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Global career rules from content/career_settings.json.
    /// </summary>
    public sealed class CareerSettings
    {
        /// <summary>
        /// Creates the settings.
        /// </summary>
        /// <param name="workActivityId">Activity whose hours count as work.</param>
        /// <param name="minWorkAge">Youngest working age.</param>
        /// <param name="retirementAge">Age of automatic retirement.</param>
        /// <param name="performance">Weekly performance weights.</param>
        /// <param name="boss">Boss rules.</param>
        /// <param name="prestigePerPoint">Hiring chance points per institution prestige point above 50.</param>
        /// <param name="retryWeeks">Weeks before applying to the same track again.</param>
        /// <param name="promotion">Promotion rules.</param>
        /// <param name="layoffs">Monthly layoff chance per economy phase id.</param>
        public CareerSettings(string workActivityId, int minWorkAge, int retirementAge, WorkPerformanceWeights performance, BossRules boss,
            float prestigePerPoint, int retryWeeks, PromotionRules promotion, IReadOnlyDictionary<string, float> layoffs)
        {
            if (minWorkAge < 0 || retirementAge <= minWorkAge || prestigePerPoint < 0f || retryWeeks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minWorkAge), "Ages, prestige points or retry weeks out of range.");
            }
            WorkActivityId = ContentId.Require(workActivityId, nameof(workActivityId));
            MinWorkAge = minWorkAge;
            RetirementAge = retirementAge;
            Performance = performance ?? throw new ArgumentNullException(nameof(performance));
            Boss = boss ?? throw new ArgumentNullException(nameof(boss));
            PrestigePerPoint = prestigePerPoint;
            RetryWeeks = retryWeeks;
            Promotion = promotion ?? throw new ArgumentNullException(nameof(promotion));
            Layoffs = layoffs ?? throw new ArgumentNullException(nameof(layoffs));
            foreach (float chance in layoffs.Values)
            {
                if (!(chance >= 0f) || chance > 1f)
                {
                    throw new ArgumentOutOfRangeException(nameof(layoffs), chance, "Layoff chances must be within 0-1.");
                }
            }
        }

        /// <summary>Activity counted as work.</summary>
        public string WorkActivityId { get; }

        /// <summary>Youngest working age.</summary>
        public int MinWorkAge { get; }

        /// <summary>Age of automatic retirement.</summary>
        public int RetirementAge { get; }

        /// <summary>Weekly performance weights.</summary>
        public WorkPerformanceWeights Performance { get; }

        /// <summary>Boss rules.</summary>
        public BossRules Boss { get; }

        /// <summary>Hiring chance points per prestige point above 50.</summary>
        public float PrestigePerPoint { get; }

        /// <summary>Weeks before applying to the same track again.</summary>
        public int RetryWeeks { get; }

        /// <summary>Promotion rules.</summary>
        public PromotionRules Promotion { get; }

        /// <summary>Monthly layoff chance per economy phase.</summary>
        public IReadOnlyDictionary<string, float> Layoffs { get; }

        /// <summary>
        /// Returns the monthly layoff chance in a phase (0 when not listed).
        /// </summary>
        /// <param name="phaseId">Economy phase id.</param>
        /// <returns>Chance 0-1.</returns>
        public float LayoffChance(string phaseId) => phaseId != null && Layoffs.TryGetValue(phaseId, out float chance) ? chance : 0f;
    }
}
