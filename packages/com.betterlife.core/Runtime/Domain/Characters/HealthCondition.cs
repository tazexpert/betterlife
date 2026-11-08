using System;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Characters
{
    /// <summary>
    /// An active illness or injury. Its overall effect on Health flows through the disease burden sub-stat.
    /// </summary>
    public sealed class HealthCondition
    {
        /// <summary>
        /// Creates a condition.
        /// </summary>
        /// <param name="conditionId">Condition content id.</param>
        /// <param name="severity">Severity, 0-100.</param>
        /// <param name="startedOn">Week it started.</param>
        public HealthCondition(string conditionId, float severity, GameDate startedOn)
        {
            if (!StatBounds.IsInRange(severity))
            {
                throw new ArgumentOutOfRangeException(nameof(severity), severity, "Severity must be within 0-100.");
            }

            ConditionId = ContentId.Require(conditionId, nameof(conditionId));
            Severity = severity;
            StartedOn = startedOn;
        }

        /// <summary>Condition content id.</summary>
        public string ConditionId { get; }

        /// <summary>Severity, 0-100.</summary>
        public float Severity { get; }

        /// <summary>Week it started.</summary>
        public GameDate StartedOn { get; }
    }
}
