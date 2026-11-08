using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a health condition.</summary>
    [Serializable]
    public sealed class HealthConditionDto
    {
        /// <summary>Condition id.</summary>
        public string Id;

        /// <summary>Severity 0-100.</summary>
        public float Severity;

        /// <summary>Start week (GameDate.TotalWeeks).</summary>
        public int StartedWeek;
    }
}
