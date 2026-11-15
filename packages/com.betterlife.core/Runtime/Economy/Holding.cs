using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Time;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// One owned asset position, valued in real base-year baht.
    /// </summary>
    public sealed class Holding
    {
        /// <summary>
        /// Creates a holding.
        /// </summary>
        /// <param name="id">Unique id within the portfolio; positive.</param>
        /// <param name="assetId">Asset id.</param>
        /// <param name="value">Current value; non-negative.</param>
        /// <param name="costBasis">Amount paid; non-negative.</param>
        /// <param name="acquiredOn">Week of purchase.</param>
        public Holding(int id, string assetId, decimal value, decimal costBasis, GameDate acquiredOn)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, "Id must be positive.");
            }
            if (value < 0m || costBasis < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Values must be non-negative.");
            }
            Id = id;
            AssetId = ContentId.Require(assetId, nameof(assetId));
            Value = value;
            CostBasis = costBasis;
            AcquiredOn = acquiredOn;
        }

        /// <summary>Unique id within the portfolio.</summary>
        public int Id { get; }

        /// <summary>Asset id.</summary>
        public string AssetId { get; }

        /// <summary>Current value.</summary>
        public decimal Value { get; private set; }

        /// <summary>Amount paid.</summary>
        public decimal CostBasis { get; private set; }

        /// <summary>Week of purchase.</summary>
        public GameDate AcquiredOn { get; }

        /// <summary>Gain or loss versus the amount paid.</summary>
        public decimal Gain => Value - CostBasis;

        internal void ApplyReturn(decimal rate)
        {
            Value = Math.Max(0m, Math.Round(Value * (1m + rate), Money.Decimals, MidpointRounding.AwayFromZero));
        }

        internal void AddValue(decimal amount)
        {
            Value += amount;
            CostBasis += amount;
        }

        internal decimal RemoveShare(decimal share)
        {
            decimal removed = Math.Round(Value * share, Money.Decimals, MidpointRounding.AwayFromZero);
            Value -= removed;
            CostBasis = Math.Round(CostBasis * (1m - share), Money.Decimals, MidpointRounding.AwayFromZero);
            return removed;
        }
    }
}
