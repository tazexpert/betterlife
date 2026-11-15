using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// A quit attempt failed.
    /// </summary>
    public sealed class RelapseEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <param name="methodId">Quit method id.</param>
        /// <param name="week">Week.</param>
        public RelapseEvent(string viceId, string methodId, GameDate week)
        {
            ViceId = viceId;
            MethodId = methodId;
            Week = week;
        }

        /// <summary>Vice id.</summary>
        public string ViceId { get; }

        /// <summary>Quit method id.</summary>
        public string MethodId { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
