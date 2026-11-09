using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// Family found out about a habit.
    /// </summary>
    public sealed class ViceDiscoveredEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <param name="week">Week.</param>
        public ViceDiscoveredEvent(string viceId, GameDate week)
        {
            ViceId = viceId;
            Week = week;
        }

        /// <summary>Vice id.</summary>
        public string ViceId { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
