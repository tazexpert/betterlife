using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// One phase of the business cycle, loaded from content/economy.json.
    /// </summary>
    public sealed class EconomyPhaseDefinition : IDefinition
    {
        /// <summary>
        /// Creates a phase.
        /// </summary>
        /// <param name="id">Phase id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="nextId">Phase that follows.</param>
        /// <param name="minYears">Shortest duration in years; at least 1.</param>
        /// <param name="maxYears">Longest duration in years.</param>
        /// <param name="inflation">Annual inflation range.</param>
        /// <param name="policyRate">Annual policy rate range (non-negative).</param>
        /// <param name="stocks">Stock market.</param>
        /// <param name="property">Property market.</param>
        /// <param name="crypto">Crypto market.</param>
        public EconomyPhaseDefinition(string id, string nameKey, string nextId, int minYears, int maxYears, FloatRange inflation, FloatRange policyRate,
            MarketParameters stocks, MarketParameters property, MarketParameters crypto)
        {
            if (minYears < 1 || maxYears < minYears)
            {
                throw new ArgumentOutOfRangeException(nameof(maxYears), maxYears, "Durations must satisfy 1 <= min <= max.");
            }
            if (inflation.Min <= -1f || policyRate.Min < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(policyRate), "Inflation must exceed -100% and the policy rate be non-negative.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            NextId = ContentId.Require(nextId, nameof(nextId));
            MinYears = minYears;
            MaxYears = maxYears;
            Inflation = inflation;
            PolicyRate = policyRate;
            Stocks = stocks ?? throw new ArgumentNullException(nameof(stocks));
            Property = property ?? throw new ArgumentNullException(nameof(property));
            Crypto = crypto ?? throw new ArgumentNullException(nameof(crypto));
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Phase that follows.</summary>
        public string NextId { get; }

        /// <summary>Shortest duration in years.</summary>
        public int MinYears { get; }

        /// <summary>Longest duration in years.</summary>
        public int MaxYears { get; }

        /// <summary>Annual inflation range.</summary>
        public FloatRange Inflation { get; }

        /// <summary>Annual policy rate range.</summary>
        public FloatRange PolicyRate { get; }

        /// <summary>Stock market.</summary>
        public MarketParameters Stocks { get; }

        /// <summary>Property market.</summary>
        public MarketParameters Property { get; }

        /// <summary>Crypto market.</summary>
        public MarketParameters Crypto { get; }
    }
}
