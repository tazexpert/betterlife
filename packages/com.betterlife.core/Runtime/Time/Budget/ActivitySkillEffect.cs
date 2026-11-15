using System;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Time.Budget
{
    /// <summary>
    /// Skill practice per hour spent on an activity, optionally capped at a number of useful hours.
    /// </summary>
    public sealed class ActivitySkillEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="skillId">Skill to practice.</param>
        /// <param name="gainPerHour">Base gain per hour; non-negative.</param>
        /// <param name="maxHours">Hours beyond this give no further gain; null for no cap.</param>
        public ActivitySkillEffect(string skillId, float gainPerHour, int? maxHours = null)
        {
            if (float.IsNaN(gainPerHour) || float.IsInfinity(gainPerHour) || gainPerHour < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(gainPerHour), gainPerHour, "Gain must be non-negative.");
            }
            if (maxHours.HasValue && maxHours.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHours), maxHours, "Max hours must be positive.");
            }
            SkillId = ContentId.Require(skillId, nameof(skillId));
            GainPerHour = gainPerHour;
            MaxHours = maxHours;
        }

        /// <summary>Skill to practice.</summary>
        public string SkillId { get; }

        /// <summary>Base gain per hour.</summary>
        public float GainPerHour { get; }

        /// <summary>Cap of useful hours, or null.</summary>
        public int? MaxHours { get; }

        /// <summary>
        /// Returns the base gain for the given hours (before the learning multiplier).
        /// </summary>
        /// <param name="hours">Hours spent.</param>
        /// <returns>The base gain.</returns>
        public float TotalFor(int hours) => GainPerHour * ActivityStatEffect.EffectiveHours(hours, MaxHours);
    }
}
