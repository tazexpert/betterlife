using System;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Monthly pocket money from a given age (until the next band starts).
    /// </summary>
    public sealed class AllowanceBand
    {
        /// <summary>
        /// Creates the band.
        /// </summary>
        /// <param name="fromAge">First age of the band.</param>
        /// <param name="monthly">Monthly allowance; non-negative.</param>
        public AllowanceBand(int fromAge, decimal monthly)
        {
            if (fromAge < 0 || monthly < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(fromAge), "Age and amount must be non-negative.");
            }
            FromAge = fromAge;
            Monthly = monthly;
        }

        /// <summary>First age of the band.</summary>
        public int FromAge { get; }

        /// <summary>Monthly allowance.</summary>
        public decimal Monthly { get; }
    }
}
