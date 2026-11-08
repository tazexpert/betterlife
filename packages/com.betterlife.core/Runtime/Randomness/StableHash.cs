using System;
using System.Text;

namespace BetterLife.Core.Randomness
{
    /// <summary>
    /// Platform-independent string hashing (FNV-1a 64-bit over UTF-8). Unlike string.GetHashCode,
    /// the result is identical across runs, devices and runtimes.
    /// </summary>
    public static class StableHash
    {
        private const ulong FnvOffsetBasis = 14695981039346656037UL;
        private const ulong FnvPrime = 1099511628211UL;

        /// <summary>
        /// Computes the FNV-1a 64-bit hash of a non-empty string.
        /// </summary>
        /// <param name="value">String to hash.</param>
        /// <returns>The 64-bit hash.</returns>
        public static ulong Fnv1a64(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException("Value must be a non-empty string.", nameof(value));
            }

            ulong hash = FnvOffsetBasis;
            foreach (byte b in Encoding.UTF8.GetBytes(value))
            {
                hash ^= b;
                hash = unchecked(hash * FnvPrime);
            }
            return hash;
        }
    }
}
