using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a held trait.</summary>
    [Serializable]
    public sealed class TraitDto
    {
        /// <summary>Trait id.</summary>
        public string Id;

        /// <summary>Acquisition week (GameDate.TotalWeeks).</summary>
        public int AcquiredWeek;

        /// <summary>Whether the player knows about it.</summary>
        public bool Revealed;
    }
}
