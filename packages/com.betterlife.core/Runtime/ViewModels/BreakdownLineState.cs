namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One line of a stat breakdown.
    /// </summary>
    public sealed class BreakdownLineState
    {
        /// <summary>
        /// Creates a line.
        /// </summary>
        /// <param name="label">What the contribution comes from.</param>
        /// <param name="detail">Extra detail (input × weight), or empty.</param>
        /// <param name="contribution">Contribution.</param>
        /// <param name="contributionText">Signed contribution text.</param>
        public BreakdownLineState(string label, string detail, float contribution, string contributionText)
        {
            Label = label;
            Detail = detail;
            Contribution = contribution;
            ContributionText = contributionText;
        }

        /// <summary>What the contribution comes from.</summary>
        public string Label { get; }

        /// <summary>Extra detail, or empty.</summary>
        public string Detail { get; }

        /// <summary>Contribution.</summary>
        public float Contribution { get; }

        /// <summary>Signed contribution text.</summary>
        public string ContributionText { get; }
    }
}
