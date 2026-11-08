namespace BetterLife.Core.Domain
{
    /// <summary>
    /// How important something is to the player. Major and Critical interrupt time skipping by default.
    /// </summary>
    public enum Severity
    {
        /// <summary>Background detail.</summary>
        Minor = 0,
        /// <summary>Worth noting in the inbox.</summary>
        Normal = 1,
        /// <summary>Needs the player's attention now.</summary>
        Major = 2,
        /// <summary>Life-changing.</summary>
        Critical = 3,
    }
}
