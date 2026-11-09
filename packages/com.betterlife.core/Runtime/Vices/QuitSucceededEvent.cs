using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// A quit attempt succeeded.
    /// </summary>
    public sealed class QuitSucceededEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <param name="methodId">Quit method id.</param>
        /// <param name="week">Week.</param>
        public QuitSucceededEvent(string viceId, string methodId, GameDate week)
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
