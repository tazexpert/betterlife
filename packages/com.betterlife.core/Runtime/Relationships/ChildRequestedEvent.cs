using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// An event effect asks the relationship service to start a child with the spouse or partner.
    /// </summary>
    public sealed class ChildRequestedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the request.
        /// </summary>
        /// <param name="week">Week of the request.</param>
        public ChildRequestedEvent(GameDate week)
        {
            Week = week;
        }

        /// <summary>Week of the request.</summary>
        public GameDate Week { get; }
    }
}
