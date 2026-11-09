using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of the character's history with one vice.</summary>
    [Serializable]
    public sealed class ViceStateDto
    {
        /// <summary>Vice id.</summary>
        public string ViceId;

        /// <summary>Addiction 0-100.</summary>
        public float Addiction;

        /// <summary>Tolerance 0-100.</summary>
        public float Tolerance;

        /// <summary>Uses so far.</summary>
        public int Uses;

        /// <summary>Whether LastUseWeek is meaningful.</summary>
        public bool HasLastUse;

        /// <summary>Week of the last use (GameDate.TotalWeeks).</summary>
        public int LastUseWeek;

        /// <summary>Bets placed.</summary>
        public int Bets;

        /// <summary>Net winnings as an invariant-culture decimal string.</summary>
        public string NetWinnings;

        /// <summary>Whether family found out.</summary>
        public bool Discovered;

        /// <summary>Failed quit attempts.</summary>
        public int Relapses;

        /// <summary>Successful quit attempts.</summary>
        public int Quits;

        /// <summary>Current quit method, or null.</summary>
        public string QuitMethodId;

        /// <summary>Quit start (GameDate.TotalWeeks).</summary>
        public int QuitStartWeek;

        /// <summary>Quit success week (GameDate.TotalWeeks).</summary>
        public int QuitEndWeek;
    }
}
