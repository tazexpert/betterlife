using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of the time state: current week and the weekly plan.</summary>
    [Serializable]
    public sealed class TimeStateDto
    {
        /// <summary>Current week (GameDate.TotalWeeks).</summary>
        public int CurrentWeek;

        /// <summary>Explicit allocations (the default activity is implied).</summary>
        public List<TimeAllocationDto> Allocations = new List<TimeAllocationDto>();
    }
}
