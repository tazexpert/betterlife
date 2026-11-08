using System;

namespace BetterLife.Core.Randomness
{
    /// <summary>
    /// PCG-XSH-RR 64/32 random generator (M. E. O'Neill, pcg-random.org). Small, fast and fully deterministic.
    /// </summary>
    public sealed class Pcg32 : IRandomProvider
    {
        private const ulong Multiplier = 6364136223846793005UL;
        private const int XorShiftBits = 18;
        private const int OutputShiftBits = 27;
        private const int RotationShiftBits = 59;
        private const int RotationMask = 31;
        private const int FloatMantissaBits = 24;
        private const int FloatDiscardBits = 32 - FloatMantissaBits;
        private const float FloatScale = 1f / (1 << FloatMantissaBits);

        private ulong _seed;
        private ulong _state;
        private ulong _increment;

        /// <summary>
        /// Creates a generator using the reference PCG seeding procedure.
        /// </summary>
        /// <param name="seed">Initial state seed.</param>
        /// <param name="sequence">Stream selector; different sequences give independent streams.</param>
        public Pcg32(ulong seed, ulong sequence)
        {
            _seed = seed;
            _increment = (sequence << 1) | 1UL;
            _state = 0UL;
            Step();
            _state = unchecked(_state + seed);
            Step();
        }

        /// <summary>
        /// Creates a generator derived from a parent seed and a stable stream name.
        /// </summary>
        /// <param name="parentSeed">Seed of the parent stream or of the life.</param>
        /// <param name="streamName">Stable, non-empty stream name.</param>
        /// <returns>The derived generator.</returns>
        public static Pcg32 CreateDerived(ulong parentSeed, string streamName)
        {
            ulong nameHash = StableHash.Fnv1a64(streamName);
            return new Pcg32(parentSeed ^ nameHash, nameHash);
        }

        /// <inheritdoc />
        public uint NextUInt()
        {
            ulong old = _state;
            Step();
            uint xorShifted = (uint)(((old >> XorShiftBits) ^ old) >> OutputShiftBits);
            int rotation = (int)(old >> RotationShiftBits);
            return (xorShifted >> rotation) | (xorShifted << ((-rotation) & RotationMask));
        }

        /// <inheritdoc />
        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive)
            {
                throw new ArgumentOutOfRangeException(nameof(maxExclusive), maxExclusive, "Max must be greater than min.");
            }

            uint range = (uint)((long)maxExclusive - minInclusive);
            // Reject the low values that would bias the modulo (reference pcg32_boundedrand).
            uint threshold = unchecked((uint)-(int)range) % range;
            uint value;
            do
            {
                value = NextUInt();
            }
            while (value < threshold);

            return (int)(minInclusive + (long)(value % range));
        }

        /// <inheritdoc />
        public float NextFloat()
        {
            return (NextUInt() >> FloatDiscardBits) * FloatScale;
        }

        /// <inheritdoc />
        public bool Chance(float probability01)
        {
            if (float.IsNaN(probability01))
            {
                throw new ArgumentException("Probability must not be NaN.", nameof(probability01));
            }

            float roll = NextFloat();
            float clamped = Math.Max(0f, Math.Min(1f, probability01));
            return roll < clamped;
        }

        /// <inheritdoc />
        public IRandomProvider Fork(string streamName)
        {
            return CreateDerived(_seed, streamName);
        }

        /// <inheritdoc />
        public RandomState CaptureState()
        {
            return new RandomState(_seed, _state, _increment);
        }

        /// <inheritdoc />
        public void RestoreState(RandomState state)
        {
            if ((state.Increment & 1UL) == 0UL)
            {
                throw new ArgumentException("Increment must be odd.", nameof(state));
            }

            _seed = state.Seed;
            _state = state.State;
            _increment = state.Increment;
        }

        private void Step()
        {
            _state = unchecked(_state * Multiplier + _increment);
        }
    }
}
