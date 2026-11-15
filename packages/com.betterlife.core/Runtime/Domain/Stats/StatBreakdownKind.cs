namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// What a breakdown explains.
    /// </summary>
    public enum StatBreakdownKind
    {
        /// <summary>How a derived stat's value is composed from its inputs.</summary>
        Formula = 0,
        /// <summary>Which sources changed a stored stat recently.</summary>
        RecentChanges = 1,
    }
}
