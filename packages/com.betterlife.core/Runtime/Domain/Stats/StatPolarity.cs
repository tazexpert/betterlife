namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Whether a high value is good or bad for the character (drives trend colors and formula inversion).
    /// </summary>
    public enum StatPolarity
    {
        /// <summary>Higher values are better, for example Fitness.</summary>
        HigherIsBetter = 0,
        /// <summary>Lower values are better, for example Stress.</summary>
        LowerIsBetter = 1,
    }
}
