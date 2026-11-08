using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Save representation of a <see cref="BetterLife.Core.Time.GameDate"/>.
    /// Uses public fields so any serializer (including Unity JsonUtility) can handle it.
    /// </summary>
    [Serializable]
    public sealed class GameDateDto
    {
        /// <summary>Weeks since the epoch.</summary>
        public int TotalWeeks;
    }
}
