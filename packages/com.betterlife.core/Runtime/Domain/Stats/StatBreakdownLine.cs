namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// One line of a stat breakdown: a labelled contribution to the final value or trend.
    /// </summary>
    public sealed class StatBreakdownLine
    {
        /// <summary>
        /// Creates a breakdown line.
        /// </summary>
        /// <param name="labelKey">Localization key describing the source.</param>
        /// <param name="contribution">Points this source contributes.</param>
        /// <param name="sourceStat">Input stat for formula lines, or null.</param>
        /// <param name="inputValue">Input stat value for formula lines, or null.</param>
        /// <param name="weight">Formula weight for formula lines, or null.</param>
        public StatBreakdownLine(string labelKey, float contribution, StatId? sourceStat = null, float? inputValue = null, float? weight = null)
        {
            LabelKey = labelKey;
            Contribution = contribution;
            SourceStat = sourceStat;
            InputValue = inputValue;
            Weight = weight;
        }

        /// <summary>Localization key describing the source.</summary>
        public string LabelKey { get; }

        /// <summary>Points this source contributes.</summary>
        public float Contribution { get; }

        /// <summary>Input stat for formula lines.</summary>
        public StatId? SourceStat { get; }

        /// <summary>Input value for formula lines (after inversion is not applied).</summary>
        public float? InputValue { get; }

        /// <summary>Formula weight for formula lines.</summary>
        public float? Weight { get; }
    }
}
