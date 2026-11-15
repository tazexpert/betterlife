namespace BetterLife.Core.Domain.Traits
{
    /// <summary>
    /// Result of trying to add a trait.
    /// </summary>
    public enum TraitAddOutcome
    {
        /// <summary>The trait was added.</summary>
        Added = 0,
        /// <summary>The character already has the trait.</summary>
        AlreadyHas = 1,
        /// <summary>The character has another trait in the same exclusive group.</summary>
        ExclusiveConflict = 2,
    }
}
