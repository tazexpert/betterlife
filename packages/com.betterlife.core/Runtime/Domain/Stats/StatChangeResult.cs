namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Outcome of applying a <see cref="StatChange"/>: requested, after trait modifiers, and after clamping.
    /// </summary>
    public sealed class StatChangeResult
    {
        /// <summary>
        /// Creates a result.
        /// </summary>
        /// <param name="statId">Changed stat.</param>
        /// <param name="requestedDelta">Delta as requested.</param>
        /// <param name="modifiedDelta">Delta after trait multipliers.</param>
        /// <param name="appliedDelta">Delta actually applied after clamping.</param>
        /// <param name="newValue">Value after the change.</param>
        public StatChangeResult(StatId statId, float requestedDelta, float modifiedDelta, float appliedDelta, float newValue)
        {
            StatId = statId;
            RequestedDelta = requestedDelta;
            ModifiedDelta = modifiedDelta;
            AppliedDelta = appliedDelta;
            NewValue = newValue;
        }

        /// <summary>Changed stat.</summary>
        public StatId StatId { get; }

        /// <summary>Delta as requested.</summary>
        public float RequestedDelta { get; }

        /// <summary>Delta after trait multipliers.</summary>
        public float ModifiedDelta { get; }

        /// <summary>Delta actually applied after clamping.</summary>
        public float AppliedDelta { get; }

        /// <summary>Value after the change.</summary>
        public float NewValue { get; }
    }
}
