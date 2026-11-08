namespace BetterLife.Core.Randomness
{
    /// <summary>
    /// Deterministic, seedable random source. All randomness in the game must go through this interface.
    /// </summary>
    public interface IRandomProvider
    {
        /// <summary>Returns the next uniformly distributed 32-bit value.</summary>
        /// <returns>A random unsigned integer.</returns>
        uint NextUInt();

        /// <summary>Returns a uniformly distributed integer in [minInclusive, maxExclusive).</summary>
        /// <param name="minInclusive">Lower bound (inclusive).</param>
        /// <param name="maxExclusive">Upper bound (exclusive); must be greater than minInclusive.</param>
        /// <returns>A random integer in range.</returns>
        int NextInt(int minInclusive, int maxExclusive);

        /// <summary>Returns a uniformly distributed float in [0, 1).</summary>
        /// <returns>A random float.</returns>
        float NextFloat();

        /// <summary>
        /// Returns true with the given probability. The input is clamped to [0, 1] and one value is always consumed,
        /// so changing a probability never shifts later results.
        /// </summary>
        /// <param name="probability01">Probability from 0 to 1.</param>
        /// <returns>True on success.</returns>
        bool Chance(float probability01);

        /// <summary>
        /// Creates an independent child stream identified by name. The result depends only on this stream's seed
        /// and the name, not on how many values were consumed.
        /// </summary>
        /// <param name="streamName">Stable, non-empty child stream name.</param>
        /// <returns>A new independent provider.</returns>
        IRandomProvider Fork(string streamName);

        /// <summary>Captures the full state for saving or undo.</summary>
        /// <returns>The current state snapshot.</returns>
        RandomState CaptureState();

        /// <summary>Restores a previously captured state.</summary>
        /// <param name="state">State to restore; its increment must be odd.</param>
        void RestoreState(RandomState state);
    }
}
