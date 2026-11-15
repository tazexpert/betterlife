namespace BetterLife.Core.Domain.Characters
{
    /// <summary>
    /// The gender the character identifies as, chosen by the player independently of biological sex.
    /// </summary>
    public enum GenderIdentity
    {
        /// <summary>Woman.</summary>
        Woman = 0,
        /// <summary>Man.</summary>
        Man = 1,
        /// <summary>Non-binary.</summary>
        NonBinary = 2,
    }
}
