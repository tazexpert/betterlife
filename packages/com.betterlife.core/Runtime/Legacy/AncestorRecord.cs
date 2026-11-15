using System;
using BetterLife.Core.Time;

namespace BetterLife.Core.Legacy
{
    /// <summary>
    /// One finished life of the family line (a node of the family tree).
    /// </summary>
    public sealed class AncestorRecord
    {
        /// <summary>
        /// Creates a record.
        /// </summary>
        /// <param name="generation">Generation (1 = founder).</param>
        /// <param name="givenName">Given name.</param>
        /// <param name="familyName">Family name.</param>
        /// <param name="bornOn">Birth week.</param>
        /// <param name="diedOn">Death week.</param>
        /// <param name="causeId">Death cause id.</param>
        /// <param name="ageYears">Age at death.</param>
        /// <param name="netWorth">Net worth at death, in the money of that time (nominal).</param>
        public AncestorRecord(int generation, string givenName, string familyName, GameDate bornOn, GameDate diedOn, string causeId, int ageYears, decimal netWorth)
        {
            if (generation < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(generation), generation, "Generation starts at 1.");
            }
            Generation = generation;
            GivenName = givenName ?? string.Empty;
            FamilyName = familyName ?? string.Empty;
            BornOn = bornOn;
            DiedOn = diedOn;
            CauseId = causeId;
            AgeYears = ageYears;
            NetWorth = netWorth;
        }

        /// <summary>Generation (1 = founder).</summary>
        public int Generation { get; }

        /// <summary>Given name.</summary>
        public string GivenName { get; }

        /// <summary>Family name.</summary>
        public string FamilyName { get; }

        /// <summary>Birth week.</summary>
        public GameDate BornOn { get; }

        /// <summary>Death week.</summary>
        public GameDate DiedOn { get; }

        /// <summary>Death cause id.</summary>
        public string CauseId { get; }

        /// <summary>Age at death.</summary>
        public int AgeYears { get; }

        /// <summary>Net worth at death, in the money of that time.</summary>
        public decimal NetWorth { get; }
    }
}
