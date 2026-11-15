namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Whether an interaction outcome brings a child.
    /// </summary>
    public enum ChildMode
    {
        /// <summary>No child.</summary>
        None = 0,

        /// <summary>A pregnancy; the child inherits both parents' genes.</summary>
        Birth = 1,

        /// <summary>An adopted child joins at once (no inherited genes).</summary>
        Adoption = 2,
    }
}
