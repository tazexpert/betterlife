using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of the daily Undo quota.</summary>
    [Serializable]
    public sealed class UndoQuotaDto
    {
        /// <summary>Day as yyyymmdd.</summary>
        public int Day;

        /// <summary>Undos used that day.</summary>
        public int Used;
    }
}
