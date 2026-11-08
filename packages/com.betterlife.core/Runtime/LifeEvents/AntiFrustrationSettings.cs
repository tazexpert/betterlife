using System;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Softens streaks of bad news: after enough recent negative events in a row, negative weights are reduced.
    /// </summary>
    public sealed class AntiFrustrationSettings
    {
        /// <summary>
        /// Creates settings.
        /// </summary>
        /// <param name="streakThreshold">Consecutive negative events that activate the reduction; at least 1.</param>
        /// <param name="windowWeeks">Only events this recent count toward the streak; positive.</param>
        /// <param name="negativeWeightMultiplier">Multiplier for negative events while active, 0-1.</param>
        public AntiFrustrationSettings(int streakThreshold, int windowWeeks, float negativeWeightMultiplier)
        {
            if (streakThreshold < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(streakThreshold), streakThreshold, "Threshold must be at least 1.");
            }
            if (windowWeeks < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(windowWeeks), windowWeeks, "Window must be positive.");
            }
            if (!(negativeWeightMultiplier >= 0f) || negativeWeightMultiplier > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(negativeWeightMultiplier), negativeWeightMultiplier, "Multiplier must be within 0-1.");
            }
            StreakThreshold = streakThreshold;
            WindowWeeks = windowWeeks;
            NegativeWeightMultiplier = negativeWeightMultiplier;
        }

        /// <summary>Consecutive negative events that activate the reduction.</summary>
        public int StreakThreshold { get; }

        /// <summary>Recency window in weeks.</summary>
        public int WindowWeeks { get; }

        /// <summary>Multiplier for negative events while active.</summary>
        public float NegativeWeightMultiplier { get; }
    }
}
