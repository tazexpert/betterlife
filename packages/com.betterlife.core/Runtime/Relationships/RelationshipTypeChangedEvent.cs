using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// A relationship changed role (dating, marriage, break-up, divorce).
    /// </summary>
    public sealed class RelationshipTypeChangedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="relationship">The relationship.</param>
        /// <param name="from">Previous role.</param>
        /// <param name="to">New role.</param>
        /// <param name="week">Week.</param>
        public RelationshipTypeChangedEvent(Relationship relationship, RelationshipType from, RelationshipType to, GameDate week)
        {
            Relationship = relationship;
            From = from;
            To = to;
            Week = week;
        }

        /// <summary>The relationship.</summary>
        public Relationship Relationship { get; }

        /// <summary>Previous role.</summary>
        public RelationshipType From { get; }

        /// <summary>New role.</summary>
        public RelationshipType To { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
