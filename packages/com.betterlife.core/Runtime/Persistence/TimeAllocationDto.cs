using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of one activity allocation.</summary>
    [Serializable]
    public sealed class TimeAllocationDto
    {
        /// <summary>Activity id.</summary>
        public string ActivityId;

        /// <summary>Weekly hours.</summary>
        public int Hours;
    }
}
