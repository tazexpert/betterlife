using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// The character was scammed.
    /// </summary>
    public sealed class ScammedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="incident">What was lost.</param>
        /// <param name="week">Week.</param>
        public ScammedEvent(ScamIncident incident, GameDate week)
        {
            Incident = incident;
            Week = week;
        }

        /// <summary>What was lost.</summary>
        public ScamIncident Incident { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
