using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// The character met someone new.
    /// </summary>
    public sealed class MetPersonEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="npc">The NPC.</param>
        /// <param name="relationship">The new relationship.</param>
        /// <param name="week">Week.</param>
        public MetPersonEvent(Npc npc, Relationship relationship, GameDate week)
        {
            Npc = npc;
            Relationship = relationship;
            Week = week;
        }

        /// <summary>The NPC.</summary>
        public Npc Npc { get; }

        /// <summary>The new relationship.</summary>
        public Relationship Relationship { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
