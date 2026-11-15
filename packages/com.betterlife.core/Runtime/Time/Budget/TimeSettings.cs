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
        /// <param name="stageBudgets">Plan per life stage id, applied when the stage begins; stages without one use the default.</param>
        public TimeSettings(string defaultActivityId, float learningAtZeroMental, float learningAtFullMental, IReadOnlyDictionary<string, int> defaultBudget,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> stageBudgets = null)
        {
            if (!(learningAtZeroMental > 0f) || !(learningAtFullMental > 0f) || float.IsInfinity(learningAtZeroMental) || float.IsInfinity(learningAtFullMental))
            {
                throw new ArgumentOutOfRangeException(nameof(learningAtZeroMental), "Learning multipliers must be positive and finite.");
            }
            DefaultActivityId = ContentId.Require(defaultActivityId, nameof(defaultActivityId));
            LearningAtZeroMental = learningAtZeroMental;
            LearningAtFullMental = learningAtFullMental;
            DefaultBudget = (defaultBudget ?? new Dictionary<string, int>()).ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);
            StageBudgets = (stageBudgets ?? new Dictionary<string, IReadOnlyDictionary<string, int>>()).ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);
        }

        /// <summary>Plan per life stage id.</summary>
        public IReadOnlyDictionary<string, IReadOnlyDictionary<string, int>> StageBudgets { get; }

        /// <summary>
        /// Returns the plan for a life stage, or the default plan.
        /// </summary>
        /// <param name="stageId">Life stage id.</param>
        /// <returns>Hours per activity.</returns>
        public IReadOnlyDictionary<string, int> BudgetFor(string stageId)
        {
            return stageId != null && StageBudgets.TryGetValue(stageId, out IReadOnlyDictionary<string, int> plan) ? plan : DefaultBudget;
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
