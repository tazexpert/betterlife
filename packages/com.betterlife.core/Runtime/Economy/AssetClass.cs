namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Kind of asset, which decides how its value changes each month.
    /// </summary>
    public enum AssetClass
    {
        /// <summary>Bank deposit: policy rate plus spread, minus inflation.</summary>
        Deposit = 0,
        /// <summary>Government bond: policy rate plus spread, minus inflation.</summary>
        Bond = 1,
        /// <summary>Index fund: follows the stock market.</summary>
        Fund = 2,
        /// <summary>Single (fictional) company: market × beta plus its own noise.</summary>
        Stock = 3,
        /// <summary>Property: follows the property market and pays rent.</summary>
        RealEstate = 4,
        /// <summary>Vehicle: depreciates.</summary>
        Vehicle = 5,
        /// <summary>Crypto currency: follows the crypto market.</summary>
        Crypto = 6,
    }
}
