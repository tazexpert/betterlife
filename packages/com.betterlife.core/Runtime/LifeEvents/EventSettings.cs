using System;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Event system tuning from content/event_settings.json.
    /// </summary>
    public sealed class EventSettings
    {
        /// <summary>
        /// Creates settings.
        /// </summary>
        /// <param name="weeklyEventChance">Probability (0-1) of drawing a random event each week.</param>
        /// <param name="responseWindowWeeks">Weeks a non-urgent event waits for an answer before the default choice is used; at least 1.</param>
        /// <param name="antiFrustration">Anti-frustration settings.</param>
        public EventSettings(float weeklyEventChance, int responseWindowWeeks, AntiFrustrationSettings antiFrustration)
        {
            if (!(weeklyEventChance >= 0f) || weeklyEventChance > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(weeklyEventChance), weeklyEventChance, "Chance must be within 0-1.");
            }
            if (responseWindowWeeks < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(responseWindowWeeks), responseWindowWeeks, "Window must be at least 1 week.");
            }
            WeeklyEventChance = weeklyEventChance;
            ResponseWindowWeeks = responseWindowWeeks;
            AntiFrustration = antiFrustration ?? throw new ArgumentNullException(nameof(antiFrustration));
        }

        /// <summary>Probability of drawing a random event each week.</summary>
        public float WeeklyEventChance { get; }

        /// <summary>Weeks a non-urgent event waits for an answer.</summary>
        public int ResponseWindowWeeks { get; }

        /// <summary>Anti-frustration settings.</summary>
        public AntiFrustrationSettings AntiFrustration { get; }
    }
}
