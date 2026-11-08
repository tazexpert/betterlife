using System;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// A request to change a stored stat, tagged with the localization key of its cause.
    /// </summary>
    public sealed class StatChange
    {
        /// <summary>
        /// Creates a change request.
        /// </summary>
        /// <param name="statId">Stored stat to change.</param>
        /// <param name="delta">Requested change; must be finite.</param>
        /// <param name="sourceKey">Localization key of the cause, for example "source.work.overtime".</param>
        public StatChange(StatId statId, float delta, string sourceKey)
        {
            if (float.IsNaN(delta) || float.IsInfinity(delta))
            {
                throw new ArgumentException("Delta must be finite.", nameof(delta));
            }

            StatId = statId;
            Delta = delta;
            SourceKey = ContentId.RequireLocKey(sourceKey, nameof(sourceKey));
        }

        /// <summary>Stat to change.</summary>
        public StatId StatId { get; }

        /// <summary>Requested change before modifiers.</summary>
        public float Delta { get; }

        /// <summary>Localization key of the cause.</summary>
        public string SourceKey { get; }
    }
}
