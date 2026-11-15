namespace BetterLife.Core.Domain.Traits
{
    /// <summary>
    /// Which direction of stat change a trait modifier scales.
    /// </summary>
    public enum ModifierDirection
    {
        /// <summary>Only increases.</summary>
        Gain = 0,
        /// <summary>Only decreases.</summary>
        Loss = 1,
        /// <summary>Both increases and decreases.</summary>
        Both = 2,
    }
}
