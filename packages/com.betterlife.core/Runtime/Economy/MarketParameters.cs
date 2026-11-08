using System;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Real (inflation-adjusted) annual return of a market: mean and volatility.
    /// </summary>
    public sealed class MarketParameters
    {
        /// <summary>
        /// Creates parameters.
        /// </summary>
        /// <param name="mean">Mean annual real return.</param>
        /// <param name="volatility">Annual standard deviation; non-negative.</param>
        public MarketParameters(float mean, float volatility)
        {
            if (float.IsNaN(mean) || float.IsInfinity(mean) || !(volatility >= 0f) || float.IsInfinity(volatility))
            {
                throw new ArgumentOutOfRangeException(nameof(volatility), "Mean must be finite and volatility non-negative.");
            }
            Mean = mean;
            Volatility = volatility;
        }

        /// <summary>Mean annual real return.</summary>
        public float Mean { get; }

        /// <summary>Annual standard deviation.</summary>
        public float Volatility { get; }
    }
}
