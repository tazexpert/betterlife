using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Something the NPC remembers about the character (a first date, a wedding, a fight...).
    /// </summary>
    public sealed class RelationshipMemory
    {
        /// <summary>
        /// Creates the memory.
        /// </summary>
        /// <param name="key">Localization key.</param>
        /// <param name="week">Week.</param>
        /// <param name="delta">Relationship change that came with it.</param>
        public RelationshipMemory(string key, GameDate week, float delta)
        {
            Key = ContentId.RequireLocKey(key, nameof(key));
            Week = week;
            Delta = delta;
        }

        /// <summary>Localization key.</summary>
        public string Key { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }

        /// <summary>Relationship change that came with it.</summary>
        public float Delta { get; }
    }
}
