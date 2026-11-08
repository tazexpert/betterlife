namespace BetterLife.Core.Randomness
{
    /// <summary>
    /// Complete snapshot of a PCG32 stream, sufficient to resume it exactly.
    /// </summary>
    public readonly struct RandomState
    {
        /// <summary>
        /// Creates a snapshot.
        /// </summary>
        /// <param name="seed">The seed the stream was created with (used to derive forks).</param>
        /// <param name="state">The current internal state.</param>
        /// <param name="increment">The stream increment; must be odd.</param>
        public RandomState(ulong seed, ulong state, ulong increment)
        {
            Seed = seed;
            State = state;
            Increment = increment;
        }

        /// <summary>The seed the stream was created with.</summary>
        public ulong Seed { get; }

        /// <summary>The current internal 64-bit state.</summary>
        public ulong State { get; }

        /// <summary>The stream increment (always odd for a valid stream).</summary>
        public ulong Increment { get; }
    }
}
