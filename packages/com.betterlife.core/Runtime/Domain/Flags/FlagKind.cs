namespace BetterLife.Core.Domain.Flags
{
    /// <summary>
    /// How a story flag stores its value.
    /// </summary>
    public enum FlagKind
    {
        /// <summary>Set or not set.</summary>
        Bool = 0,
        /// <summary>A non-negative count.</summary>
        Counter = 1,
    }
}
