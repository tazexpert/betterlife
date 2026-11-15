namespace BetterLife.Core.Time.Budget
{
    /// <summary>
    /// Why a time budget is not valid.
    /// </summary>
    public enum BudgetIssueKind
    {
        /// <summary>Allocated hours exceed the week.</summary>
        OverWeek = 0,
        /// <summary>An available activity has fewer than its minimum hours.</summary>
        BelowMinimum = 1,
        /// <summary>An activity has more than its maximum hours.</summary>
        AboveMaximum = 2,
        /// <summary>Hours are allocated to an activity whose condition fails.</summary>
        Unavailable = 3,
    }
}
