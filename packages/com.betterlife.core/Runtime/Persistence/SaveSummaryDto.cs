using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>What a save slot shows without loading the whole life.</summary>
    [Serializable]
    public sealed class SaveSummaryDto
    {
        /// <summary>Given name.</summary>
        public string GivenName;

        /// <summary>Family name.</summary>
        public string FamilyName;

        /// <summary>Current week (GameDate.TotalWeeks).</summary>
        public int Week;

        /// <summary>Age in years.</summary>
        public int AgeYears;

        /// <summary>Whether the character is alive.</summary>
        public bool Alive;
    }
}
