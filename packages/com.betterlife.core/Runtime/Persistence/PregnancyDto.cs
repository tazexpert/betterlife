using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a pregnancy.</summary>
    [Serializable]
    public sealed class PregnancyDto
    {
        /// <summary>The other parent.</summary>
        public int PartnerNpcId;

        /// <summary>Birth week (GameDate.TotalWeeks).</summary>
        public int DueWeek;
    }
}
