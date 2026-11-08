using System;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Something the character can own, loaded from content/assets.json. Amounts are real base-year baht.
    /// </summary>
    public sealed class AssetDefinition : IDefinition
    {
        /// <summary>
        /// Creates an asset.
        /// </summary>
        /// <param name="id">Asset id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="assetClass">Asset class.</param>
        /// <param name="minPurchase">Smallest purchase; positive.</param>
        /// <param name="sellFeeRate">Share of the value lost when selling, 0-1.</param>
        /// <param name="rateSpread">Deposits and bonds: annual rate above the policy rate.</param>
        /// <param name="beta">Stocks: sensitivity to the market.</param>
        /// <param name="idiosyncraticVolatility">Stocks: annual volatility of the company itself; non-negative.</param>
        /// <param name="rentalYield">Real estate: annual rent as a share of value, 0-1.</param>
        /// <param name="depreciation">Vehicles: annual loss of value, 0-1.</param>
        public AssetDefinition(string id, string nameKey, AssetClass assetClass, decimal minPurchase, decimal sellFeeRate,
            float rateSpread = 0f, float beta = 1f, float idiosyncraticVolatility = 0f, float rentalYield = 0f, float depreciation = 0f)
        {
            if (minPurchase <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(minPurchase), minPurchase, "Minimum purchase must be positive.");
            }
            if (sellFeeRate < 0m || sellFeeRate > 1m)
            {
                throw new ArgumentOutOfRangeException(nameof(sellFeeRate), sellFeeRate, "Fee must be within 0-1.");
            }
            RequireRange(rentalYield, nameof(rentalYield));
            RequireRange(depreciation, nameof(depreciation));
            if (!(idiosyncraticVolatility >= 0f) || float.IsInfinity(idiosyncraticVolatility) || float.IsNaN(rateSpread) || float.IsNaN(beta))
            {
                throw new ArgumentOutOfRangeException(nameof(idiosyncraticVolatility), "Market parameters must be finite and volatility non-negative.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Class = assetClass;
            MinPurchase = minPurchase;
            SellFeeRate = sellFeeRate;
            RateSpread = rateSpread;
            Beta = beta;
            IdiosyncraticVolatility = idiosyncraticVolatility;
            RentalYield = rentalYield;
            Depreciation = depreciation;
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Asset class.</summary>
        public AssetClass Class { get; }

        /// <summary>Smallest purchase.</summary>
        public decimal MinPurchase { get; }

        /// <summary>Share of value lost when selling.</summary>
        public decimal SellFeeRate { get; }

        /// <summary>Annual rate above the policy rate (deposits, bonds).</summary>
        public float RateSpread { get; }

        /// <summary>Sensitivity to the market (stocks).</summary>
        public float Beta { get; }

        /// <summary>Annual volatility of the company (stocks).</summary>
        public float IdiosyncraticVolatility { get; }

        /// <summary>Annual rent as a share of value (real estate).</summary>
        public float RentalYield { get; }

        /// <summary>Annual loss of value (vehicles).</summary>
        public float Depreciation { get; }

        private static void RequireRange(float value, string name)
        {
            if (!(value >= 0f) || value > 1f)
            {
                throw new ArgumentOutOfRangeException(name, value, "Value must be within 0-1.");
            }
        }
    }
}
