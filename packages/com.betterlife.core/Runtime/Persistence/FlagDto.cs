using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of an active flag. HasExpiry replaces a nullable field for JsonUtility compatibility.</summary>
    [Serializable]
    public sealed class FlagDto
    {
        /// <summary>Flag id.</summary>
        public string Id;

        /// <summary>1 for bool flags, the count for counters.</summary>
        public int Value;

        /// <summary>Week last set (GameDate.TotalWeeks).</summary>
        public int SetWeek;

        /// <summary>Whether ExpiresWeek is meaningful.</summary>
        public bool HasExpiry;

        /// <summary>Expiry week (GameDate.TotalWeeks).</summary>
        public int ExpiresWeek;
    }
}
