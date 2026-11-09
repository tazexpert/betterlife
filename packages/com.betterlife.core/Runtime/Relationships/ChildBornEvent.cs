using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// A child was born or adopted.
    /// </summary>
    public sealed class ChildBornEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="child">The child.</param>
        /// <param name="otherParentId">The other parent's NPC id (0 when adopted alone).</param>
        /// <param name="week">Week.</param>
        public ChildBornEvent(Npc child, int otherParentId, GameDate week)
        {
            Child = child;
            OtherParentId = otherParentId;
            Week = week;
        }

        /// <summary>The child.</summary>
        public Npc Child { get; }

        /// <summary>The other parent's NPC id (0 when adopted alone).</summary>
        public int OtherParentId { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
