using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of the character's history with scammers.</summary>
    [Serializable]
    public sealed class ScamsDto
    {
        /// <summary>Recent incidents.</summary>
        public List<ScamIncidentDto> Incidents = new List<ScamIncidentDto>();

        /// <summary>Times targeted.</summary>
        public int Targeted;

        /// <summary>Times spotted.</summary>
        public int Detected;

        /// <summary>Times scammed.</summary>
        public int Victimized;

        /// <summary>Money lost overall as an invariant-culture decimal string.</summary>
        public string NetLost;

        /// <summary>Next incident id.</summary>
        public int NextIncidentId = 1;
    }
}
