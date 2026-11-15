using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Someone in the character's life died.
    /// </summary>
    public sealed class NpcDiedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="npc">The NPC.</param>
        /// <param name="relationship">The relationship.</param>
        /// <param name="week">Week.</param>
        public NpcDiedEvent(Npc npc, Relationship relationship, GameDate week)
        {
            Npc = npc;
            Relationship = relationship;
            Week = week;
        }

        /// <summary>The NPC.</summary>
        public Npc Npc { get; }

        /// <summary>The relationship.</summary>
        public Relationship Relationship { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
