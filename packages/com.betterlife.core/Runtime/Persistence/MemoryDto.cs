using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a relationship memory.</summary>
    [Serializable]
    public sealed class MemoryDto
    {
        /// <summary>Localization key.</summary>
        public string Key;

        /// <summary>Week (GameDate.TotalWeeks).</summary>
        public int Week;

        /// <summary>Relationship change.</summary>
        public float Delta;
    }
}
