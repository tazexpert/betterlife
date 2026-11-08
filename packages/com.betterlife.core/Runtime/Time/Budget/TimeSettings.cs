using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Time.Budget
{
    /// <summary>
    /// Global time budget settings, loaded from content/time_settings.json.
    /// </summary>
    public sealed class TimeSettings
    {
        /// <summary>
        /// Creates settings.
        /// </summary>
        /// <param name="defaultActivityId">Activity that receives unallocated hours.</param>
        /// <param name="learningAtZeroMental">Skill gain multiplier at Mental 0; positive.</param>
        /// <param name="learningAtFullMental">Skill gain multiplier at Mental 100; positive.</param>
        /// <param name="defaultBudget">Starting hours per activity (excluding the default activity).</param>
        public TimeSettings(string defaultActivityId, float learningAtZeroMental, float learningAtFullMental, IReadOnlyDictionary<string, int> defaultBudget)
        {
            if (!(learningAtZeroMental > 0f) || !(learningAtFullMental > 0f) || float.IsInfinity(learningAtZeroMental) || float.IsInfinity(learningAtFullMental))
            {
                throw new ArgumentOutOfRangeException(nameof(learningAtZeroMental), "Learning multipliers must be positive and finite.");
            }
            DefaultActivityId = ContentId.Require(defaultActivityId, nameof(defaultActivityId));
            LearningAtZeroMental = learningAtZeroMental;
            LearningAtFullMental = learningAtFullMental;
            DefaultBudget = (defaultBudget ?? new Dictionary<string, int>()).ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);
        }

        /// <summary>Activity that receives unallocated hours.</summary>
        public string DefaultActivityId { get; }

        /// <summary>Skill gain multiplier at Mental 0.</summary>
        public float LearningAtZeroMental { get; }

        /// <summary>Skill gain multiplier at Mental 100.</summary>
        public float LearningAtFullMental { get; }

        /// <summary>Starting hours per activity.</summary>
        public IReadOnlyDictionary<string, int> DefaultBudget { get; }

        /// <summary>
        /// Returns the skill gain multiplier for a Mental value (linear between the two settings).
        /// </summary>
        /// <param name="mental">Mental, 0-100.</param>
        /// <returns>The multiplier.</returns>
        public float LearningMultiplier(float mental)
        {
            float t = StatBounds.Clamp(mental) / StatBounds.Max;
            return LearningAtZeroMental + (LearningAtFullMental - LearningAtZeroMental) * t;
        }
    }
}
