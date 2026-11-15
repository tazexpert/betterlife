using System.Collections.Generic;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One core stat card on the dashboard.
    /// </summary>
    public sealed class StatCardState
    {
        /// <summary>
        /// Creates a card.
        /// </summary>
        /// <param name="statId">Stat id.</param>
        /// <param name="name">Stat name.</param>
        /// <param name="value">Current value 0..100.</param>
        /// <param name="valueText">Value text.</param>
        /// <param name="delta">Change over the trend window.</param>
        /// <param name="deltaText">Change text (empty when flat).</param>
        /// <param name="direction">Trend direction.</param>
        /// <param name="isImproving">Whether the trend is good for the character (uses the stat polarity).</param>
        /// <param name="sparkline">Sparkline points.</param>
        public StatCardState(StatId statId, string name, float value, string valueText, float delta, string deltaText, TrendDirection direction,
            bool isImproving, IReadOnlyList<SparkPoint> sparkline)
        {
            StatId = statId;
            Name = name;
            Value = value;
            ValueText = valueText;
            Delta = delta;
            DeltaText = deltaText;
            Direction = direction;
            IsImproving = isImproving;
            Sparkline = sparkline;
        }

        /// <summary>Stat id.</summary>
        public StatId StatId { get; }

        /// <summary>Stat name.</summary>
        public string Name { get; }

        /// <summary>Current value 0..100.</summary>
        public float Value { get; }

        /// <summary>Value text.</summary>
        public string ValueText { get; }

        /// <summary>Change over the trend window.</summary>
        public float Delta { get; }

        /// <summary>Change text (empty when flat).</summary>
        public string DeltaText { get; }

        /// <summary>Trend direction.</summary>
        public TrendDirection Direction { get; }

        /// <summary>Whether the trend is good for the character.</summary>
        public bool IsImproving { get; }

        /// <summary>Sparkline points.</summary>
        public IReadOnlyList<SparkPoint> Sparkline { get; }
    }
}
