using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Time;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// The character's history with scammers.
    /// </summary>
    public sealed class CharacterScams
    {
        private readonly List<ScamIncident> _incidents = new List<ScamIncident>();

        /// <summary>Recent incidents, oldest first.</summary>
        public IReadOnlyList<ScamIncident> Incidents => _incidents;

        /// <summary>Times targeted.</summary>
        public int Targeted { get; private set; }

        /// <summary>Times the scam was spotted.</summary>
        public int Detected { get; private set; }

        /// <summary>Times scammed (including incidents no longer kept).</summary>
        public int Victimized { get; private set; }

        /// <summary>Money lost overall (minus recoveries).</summary>
        public decimal NetLost { get; private set; }

        /// <summary>Next incident id.</summary>
        public int NextIncidentId { get; private set; } = 1;

        /// <summary>
        /// Returns an incident, or null.
        /// </summary>
        /// <param name="id">Incident id.</param>
        /// <returns>The incident.</returns>
        public ScamIncident Find(int id) => _incidents.FirstOrDefault(i => i.Id == id);

        /// <summary>
        /// Records being targeted.
        /// </summary>
        /// <param name="detected">True when the scam was spotted.</param>
        public void RecordAttempt(bool detected)
        {
            Targeted++;
            if (detected)
            {
                Detected++;
            }
        }

        /// <summary>
        /// Records a loss (keeps the newest incidents only).
        /// </summary>
        /// <param name="scamId">Scam id.</param>
        /// <param name="week">Week.</param>
        /// <param name="lost">Money lost.</param>
        /// <param name="maxIncidents">Incidents kept.</param>
        /// <returns>The incident.</returns>
        public ScamIncident AddIncident(string scamId, GameDate week, decimal lost, int maxIncidents)
        {
            var incident = new ScamIncident(NextIncidentId++, scamId, week, lost);
            _incidents.Add(incident);
            while (_incidents.Count > Math.Max(1, maxIncidents))
            {
                _incidents.RemoveAt(0);
            }
            Victimized++;
            NetLost += lost;
            return incident;
        }

        internal void Report(ScamIncident incident, decimal recovered)
        {
            incident.MarkReported(recovered);
            NetLost -= recovered;
        }

        /// <summary>
        /// Restores saved state.
        /// </summary>
        /// <param name="incidents">Incidents.</param>
        /// <param name="targeted">Times targeted.</param>
        /// <param name="detected">Times spotted.</param>
        /// <param name="victimized">Times scammed.</param>
        /// <param name="netLost">Money lost overall.</param>
        /// <param name="nextIncidentId">Next id; raised above every saved id.</param>
        public void Restore(IEnumerable<ScamIncident> incidents, int targeted, int detected, int victimized, decimal netLost, int nextIncidentId)
        {
            _incidents.Clear();
            _incidents.AddRange(incidents ?? Enumerable.Empty<ScamIncident>());
            Targeted = Math.Max(0, targeted);
            Detected = Math.Max(0, detected);
            Victimized = Math.Max(Math.Max(0, victimized), _incidents.Count);
            NetLost = netLost;
            NextIncidentId = Math.Max(Math.Max(1, nextIncidentId), _incidents.Select(i => i.Id).DefaultIfEmpty(0).Max() + 1);
        }
    }
}
