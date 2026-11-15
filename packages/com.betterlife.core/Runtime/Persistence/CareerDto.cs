using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a character's working life.</summary>
    [Serializable]
    public sealed class CareerDto
    {
        /// <summary>Current job, or null.</summary>
        public JobDto Current;

        /// <summary>Past jobs.</summary>
        public List<PastJobDto> History = new List<PastJobDto>();

        /// <summary>Whether retired.</summary>
        public bool Retired;

        /// <summary>Monthly pension as an invariant-culture decimal string.</summary>
        public string PensionMonthly;

        /// <summary>Job applications.</summary>
        public List<AttemptDto> Attempts = new List<AttemptDto>();
    }
}
