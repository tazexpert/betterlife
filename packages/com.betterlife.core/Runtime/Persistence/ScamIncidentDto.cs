using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a scam incident.</summary>
    [Serializable]
    public sealed class ScamIncidentDto
    {
        /// <summary>Incident id.</summary>
        public int Id;

        /// <summary>Scam id.</summary>
        public string ScamId;

        /// <summary>Week (GameDate.TotalWeeks).</summary>
        public int Week;

        /// <summary>Money lost as an invariant-culture decimal string.</summary>
        public string Lost;

        /// <summary>Money recovered as an invariant-culture decimal string.</summary>
        public string Recovered;

        /// <summary>Whether reported.</summary>
        public bool Reported;
    }
}
