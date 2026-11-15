using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of one stat's sparkline samples.</summary>
    [Serializable]
    public sealed class StatHistoryDto
    {
        /// <summary>Stat id.</summary>
        public string StatId;

        /// <summary>Samples, oldest first.</summary>
        public List<float> Values = new List<float>();
    }
}
