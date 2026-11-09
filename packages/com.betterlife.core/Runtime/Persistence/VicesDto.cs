using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of the character's habits.</summary>
    [Serializable]
    public sealed class VicesDto
    {
        /// <summary>States.</summary>
        public List<ViceStateDto> States = new List<ViceStateDto>();

        /// <summary>Whether LastIrreversibleWeek is meaningful.</summary>
        public bool HasIrreversible;

        /// <summary>Last week that may not be undone (GameDate.TotalWeeks).</summary>
        public int LastIrreversibleWeek;

        /// <summary>Actions so far that may never be undone.</summary>
        public int IrreversibleActions;
    }
}
