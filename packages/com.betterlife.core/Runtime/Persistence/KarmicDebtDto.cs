using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a karmic debt.</summary>
    [Serializable]
    public sealed class KarmicDebtDto
    {
        /// <summary>Remaining amount.</summary>
        public float Amount;

        /// <summary>Due week (GameDate.TotalWeeks).</summary>
        public int DueWeek;

        /// <summary>Localization key of the deed.</summary>
        public string SourceKey;
    }
}
