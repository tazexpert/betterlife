namespace BetterLife.Core.Time.Budget
{
    /// <summary>
    /// One problem with a time budget, for the UI to highlight.
    /// </summary>
    public sealed class BudgetIssue
    {
        /// <summary>
        /// Creates an issue.
        /// </summary>
        /// <param name="kind">Kind of problem.</param>
        /// <param name="activityId">Affected activity, or null for the whole budget.</param>
        /// <param name="limit">The limit that was broken (hours).</param>
        public BudgetIssue(BudgetIssueKind kind, string activityId, int limit)
        {
            Kind = kind;
            ActivityId = activityId;
            Limit = limit;
        }

        /// <summary>Kind of problem.</summary>
        public BudgetIssueKind Kind { get; }

        /// <summary>Affected activity, or null.</summary>
        public string ActivityId { get; }

        /// <summary>The limit that was broken (hours).</summary>
        public int Limit { get; }
    }
}
