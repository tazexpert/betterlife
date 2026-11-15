using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// An effect decided that the character meets someone new; the relationship service creates the person.
    /// </summary>
    public sealed class MeetRequestedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="type">Kind of relationship to start.</param>
        /// <param name="week">Week of the meeting.</param>
        public MeetRequestedEvent(RelationshipType type, GameDate week)
        {
            Type = type;
            Week = week;
        }

        /// <summary>Kind of relationship to start.</summary>
        public RelationshipType Type { get; }

        /// <summary>Week of the meeting.</summary>
        public GameDate Week { get; }
    }
}
