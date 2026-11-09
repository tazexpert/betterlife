using BetterLife.Core.Events;
using BetterLife.Core.Time;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// A scam was reported to the police and the bank.
    /// </summary>
    public sealed class ScamReportedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="incident">The incident.</param>
        /// <param name="recovered">Money returned.</param>
        /// <param name="week">Week.</param>
        public ScamReportedEvent(ScamIncident incident, decimal recovered, GameDate week)
        {
            Incident = incident;
            Recovered = recovered;
            Week = week;
        }

        /// <summary>The incident.</summary>
        public ScamIncident Incident { get; }

        /// <summary>Money returned.</summary>
        public decimal Recovered { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
