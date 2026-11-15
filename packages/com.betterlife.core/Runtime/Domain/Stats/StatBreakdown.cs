using System.Collections.Generic;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Explanation of a stat value, shown when the player long-presses a number.
    /// </summary>
    public sealed class StatBreakdown
    {
        /// <summary>
        /// Creates a breakdown.
        /// </summary>
        /// <param name="statId">The explained stat.</param>
        /// <param name="value">Its current value.</param>
        /// <param name="kind">What the lines explain.</param>
        /// <param name="lines">Contributions.</param>
        public StatBreakdown(StatId statId, float value, StatBreakdownKind kind, IReadOnlyList<StatBreakdownLine> lines)
        {
            StatId = statId;
            Value = value;
            Kind = kind;
            Lines = lines ?? new List<StatBreakdownLine>();
        }

        /// <summary>The explained stat.</summary>
        public StatId StatId { get; }

        /// <summary>Current value.</summary>
        public float Value { get; }

        /// <summary>What the lines explain.</summary>
        public StatBreakdownKind Kind { get; }

        /// <summary>Contributions in display order.</summary>
        public IReadOnlyList<StatBreakdownLine> Lines { get; }
    }
}
