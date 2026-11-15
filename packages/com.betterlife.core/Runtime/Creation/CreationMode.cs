namespace BetterLife.Core.Creation
{
    /// <summary>
    /// How a new life is created.
    /// </summary>
    public enum CreationMode
    {
        /// <summary>Everything is rolled from a new seed; no overrides.</summary>
        Random = 0,
        /// <summary>The player sets some details; the rest is rolled.</summary>
        Custom = 1,
        /// <summary>Everything is rolled from a shared seed code; no overrides, so everyone gets the same life.</summary>
        Challenge = 2,
    }
}
