using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a finished occurrence.</summary>
    [Serializable]
    public sealed class EventHistoryDto
    {
        /// <summary>Occurrence id.</summary>
        public int InstanceId;

        /// <summary>Event id.</summary>
        public string EventId;

        /// <summary>EventOrigin name.</summary>
        public string Origin;

        /// <summary>Week resolved.</summary>
        public int Week;

        /// <summary>Chosen option, or null.</summary>
        public string ChoiceId;

        /// <summary>Whether it succeeded.</summary>
        public bool Succeeded;
    }
}
