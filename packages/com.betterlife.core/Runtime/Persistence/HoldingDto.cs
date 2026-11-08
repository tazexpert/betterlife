using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of an investment holding.</summary>
    [Serializable]
    public sealed class HoldingDto
    {
        /// <summary>Holding id.</summary>
        public int Id;

        /// <summary>Asset id.</summary>
        public string AssetId;

        /// <summary>Market value as an invariant-culture decimal string.</summary>
        public string Value;

        /// <summary>Cost basis as an invariant-culture decimal string.</summary>
        public string CostBasis;

        /// <summary>Purchase week (GameDate.TotalWeeks).</summary>
        public int AcquiredWeek;
    }
}
