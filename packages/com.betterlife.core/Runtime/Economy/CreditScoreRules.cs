using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// How payments move the credit score (0-100).
    /// </summary>
    public sealed class CreditScoreRules
    {
        /// <summary>
        /// Creates rules.
        /// </summary>
        /// <param name="onTimeGain">Gain per month with every payment on time.</param>
        /// <param name="missedLoss">Loss per missed payment.</param>
        /// <param name="defaultLoss">Loss when a loan defaults.</param>
        /// <param name="foreclosureLoss">Loss when collateral is seized.</param>
        /// <param name="bankruptcyScore">Score after bankruptcy (0-100).</param>
        /// <param name="highUtilization">Card utilization above which the score drops, 0-1.</param>
        /// <param name="utilizationLoss">Monthly loss at high utilization.</param>
        public CreditScoreRules(float onTimeGain, float missedLoss, float defaultLoss, float foreclosureLoss, float bankruptcyScore, float highUtilization, float utilizationLoss)
        {
            foreach (float value in new[] { onTimeGain, missedLoss, defaultLoss, foreclosureLoss, utilizationLoss })
            {
                if (!(value >= 0f) || float.IsInfinity(value))
                {
                    throw new ArgumentOutOfRangeException(nameof(onTimeGain), value, "Changes must be non-negative.");
                }
            }
            if (!StatBounds.IsInRange(bankruptcyScore) || !(highUtilization >= 0f) || highUtilization > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(bankruptcyScore), "Score must be within 0-100 and utilization within 0-1.");
            }
            OnTimeGain = onTimeGain;
            MissedLoss = missedLoss;
            DefaultLoss = defaultLoss;
            ForeclosureLoss = foreclosureLoss;
            BankruptcyScore = bankruptcyScore;
            HighUtilization = highUtilization;
            UtilizationLoss = utilizationLoss;
        }

        /// <summary>Gain per month with every payment on time.</summary>
        public float OnTimeGain { get; }

        /// <summary>Loss per missed payment.</summary>
        public float MissedLoss { get; }

        /// <summary>Loss when a loan defaults.</summary>
        public float DefaultLoss { get; }

        /// <summary>Loss when collateral is seized.</summary>
        public float ForeclosureLoss { get; }

        /// <summary>Score after bankruptcy.</summary>
        public float BankruptcyScore { get; }

        /// <summary>Card utilization threshold.</summary>
        public float HighUtilization { get; }

        /// <summary>Monthly loss at high utilization.</summary>
        public float UtilizationLoss { get; }
    }
}
