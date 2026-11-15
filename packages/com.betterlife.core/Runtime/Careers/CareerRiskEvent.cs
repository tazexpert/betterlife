using BetterLife.Core.Events;
using BetterLife.Core.Rules;
using BetterLife.Core.Time;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// A career risk happened (arrest, injury, scandal...).
    /// </summary>
    public sealed class CareerRiskEvent : IGameEvent
    {
        /// <summary>
        /// Creates the event.
        /// </summary>
        /// <param name="trackId">Career track id.</param>
        /// <param name="reasonKey">Localization key of what happened.</param>
        /// <param name="result">Effects that were applied.</param>
        /// <param name="week">Week.</param>
        public CareerRiskEvent(string trackId, string reasonKey, EffectResult result, GameDate week)
        {
            TrackId = trackId;
            ReasonKey = reasonKey;
            Result = result;
            Week = week;
        }

        /// <summary>Career track id.</summary>
        public string TrackId { get; }

        /// <summary>Localization key of what happened.</summary>
        public string ReasonKey { get; }

        /// <summary>Effects that were applied.</summary>
        public EffectResult Result { get; }

        /// <summary>Week.</summary>
        public GameDate Week { get; }
    }
}
