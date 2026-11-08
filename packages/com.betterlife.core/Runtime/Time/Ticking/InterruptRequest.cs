using System;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// A phase's request to stop time skipping so the player can react.
    /// </summary>
    public sealed class InterruptRequest
    {
        /// <summary>
        /// Creates a request.
        /// </summary>
        /// <param name="severity">How important the cause is.</param>
        /// <param name="reasonKey">Localization key describing the cause.</param>
        /// <param name="sourceId">Id of the cause, for example an event id.</param>
        /// <param name="week">Week the request was made.</param>
        public InterruptRequest(Severity severity, string reasonKey, string sourceId, GameDate week)
        {
            Severity = severity;
            ReasonKey = ContentId.RequireLocKey(reasonKey, nameof(reasonKey));
            SourceId = sourceId ?? throw new ArgumentNullException(nameof(sourceId));
            Week = week;
        }

        /// <summary>How important the cause is.</summary>
        public Severity Severity { get; }

        /// <summary>Localization key describing the cause.</summary>
        public string ReasonKey { get; }

        /// <summary>Id of the cause.</summary>
        public string SourceId { get; }

        /// <summary>Week the request was made.</summary>
        public GameDate Week { get; }
    }
}
