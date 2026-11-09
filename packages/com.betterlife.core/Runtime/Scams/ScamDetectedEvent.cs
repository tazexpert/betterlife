using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// The character spotted a scam and lost nothing.
    /// </summary>
    public sealed class ScamDetectedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="scamId">Scam id.</param>
        /// <param name="week">Week.</param>
        public ScamDetectedEvent(string scamId, GameDate week)
        {
            ScamId = scamId;
            Week = week;
        }

        /// <summary>Scam id.</summary>
        public string ScamId { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
