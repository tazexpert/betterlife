namespace BetterLife.Core.Economy
{
    /// <summary>
    /// One month of market movement (real returns) and the month's inflation.
    /// </summary>
    public sealed class MarketMonth
    {
        /// <summary>
        /// Creates a month.
        /// </summary>
        /// <param name="stocks">Real stock index return.</param>
        /// <param name="property">Real property return.</param>
        /// <param name="crypto">Real crypto return.</param>
        /// <param name="inflation">Monthly inflation.</param>
        /// <param name="policyRate">Annual policy rate.</param>
        public MarketMonth(decimal stocks, decimal property, decimal crypto, decimal inflation, float policyRate)
        {
            Stocks = stocks;
            Property = property;
            Crypto = crypto;
            Inflation = inflation;
            PolicyRate = policyRate;
        }

        /// <summary>Real stock index return.</summary>
        public decimal Stocks { get; }

        /// <summary>Real property return.</summary>
        public decimal Property { get; }

        /// <summary>Real crypto return.</summary>
        public decimal Crypto { get; }

        /// <summary>Monthly inflation.</summary>
        public decimal Inflation { get; }

        /// <summary>Annual policy rate.</summary>
        public float PolicyRate { get; }
    }
}
