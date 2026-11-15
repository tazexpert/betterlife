using System;
using System.Globalization;
using BetterLife.Core.Legacy;
using BetterLife.Core.Time;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Saved ancestor of the family line.
    /// </summary>
    [Serializable]
    public sealed class AncestorDto
    {
        /// <summary>Generation (1 = founder).</summary>
        public int Generation;

        /// <summary>Given name.</summary>
        public string GivenName;

        /// <summary>Family name.</summary>
        public string FamilyName;

        /// <summary>Birth week (GameDate.TotalWeeks).</summary>
        public int BirthWeek;

        /// <summary>Death week (GameDate.TotalWeeks).</summary>
        public int DeathWeek;

        /// <summary>Death cause id.</summary>
        public string CauseId;

        /// <summary>Age at death.</summary>
        public int AgeYears;

        /// <summary>Net worth at death in the money of that time (decimal string).</summary>
        public string NetWorth;

        /// <summary>
        /// Converts a record.
        /// </summary>
        /// <param name="record">Record.</param>
        /// <returns>The DTO.</returns>
        public static AncestorDto From(AncestorRecord record)
        {
            return new AncestorDto
            {
                Generation = record.Generation,
                GivenName = record.GivenName,
                FamilyName = record.FamilyName,
                BirthWeek = record.BornOn.TotalWeeks,
                DeathWeek = record.DiedOn.TotalWeeks,
                CauseId = record.CauseId,
                AgeYears = record.AgeYears,
                NetWorth = record.NetWorth.ToString(CultureInfo.InvariantCulture),
            };
        }

        /// <summary>
        /// Converts back to a record (bad numbers become 0; generation at least 1).
        /// </summary>
        /// <returns>The record.</returns>
        public AncestorRecord ToRecord()
        {
            decimal.TryParse(NetWorth, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal netWorth);
            return new AncestorRecord(Math.Max(1, Generation), GivenName, FamilyName, new GameDate(BirthWeek), new GameDate(DeathWeek), CauseId, AgeYears, netWorth);
        }
    }
}
