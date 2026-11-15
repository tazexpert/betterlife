using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Time.Budget
{
    /// <summary>
    /// Linear stat change per hour spent on an activity, optionally capped at a number of useful hours.
    /// </summary>
    public sealed class ActivityStatEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="statId">Stored stat to change.</param>
        /// <param name="perHour">Change per hour; finite.</param>
        /// <param name="maxHours">Hours beyond this give no further effect; null for no cap.</param>
        public ActivityStatEffect(StatId statId, float perHour, int? maxHours = null)
        {
            if (float.IsNaN(perHour) || float.IsInfinity(perHour))
            {
                throw new ArgumentException("Per-hour change must be finite.", nameof(perHour));
            }
            if (maxHours.HasValue && maxHours.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHours), maxHours, "Max hours must be positive.");
            }
            StatId = statId;
            PerHour = perHour;
            MaxHours = maxHours;
        }

        /// <summary>Stat to change.</summary>
        public StatId StatId { get; }

        /// <summary>Change per hour.</summary>
        public float PerHour { get; }

        /// <summary>Cap of useful hours, or null.</summary>
        public int? MaxHours { get; }

        /// <summary>
        /// Returns the total change for the given hours.
        /// </summary>
        /// <param name="hours">Hours spent.</param>
        /// <returns>The change.</returns>
        public float TotalFor(int hours) => PerHour * EffectiveHours(hours, MaxHours);

        internal static int EffectiveHours(int hours, int? cap) => cap.HasValue ? Math.Min(hours, cap.Value) : hours;
    }
}
