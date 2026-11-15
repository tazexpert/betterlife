using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of the last application week for a program or track.</summary>
    [Serializable]
    public sealed class AttemptDto
    {
        /// <summary>Program or track id.</summary>
        public string Id;

        /// <summary>Week (GameDate.TotalWeeks).</summary>
        public int Week;
    }
}
