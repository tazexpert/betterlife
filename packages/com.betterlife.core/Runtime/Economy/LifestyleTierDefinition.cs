using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Family;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// A standard of living, loaded from content/lifestyle_tiers.json.
    /// </summary>
    public sealed class LifestyleTierDefinition : IDefinition
    {
        /// <summary>
        /// Creates a tier.
        /// </summary>
        /// <param name="id">Tier id.</param>
        /// <param name="nameKey">Localization key of the name (also the source of its effects).</param>
        /// <param name="monthlyCost">Real monthly cost before the local cost of living; non-negative.</param>
        /// <param name="monthlyStats">Stat changes per month.</param>
        public LifestyleTierDefinition(string id, string nameKey, decimal monthlyCost, IEnumerable<StatAmount> monthlyStats)
        {
            if (monthlyCost < 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(monthlyCost), monthlyCost, "Cost must be non-negative.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            MonthlyCost = monthlyCost;
            MonthlyStats = (monthlyStats ?? Enumerable.Empty<StatAmount>()).ToList();
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name.</summary>
        public string NameKey { get; }

        /// <summary>Real monthly cost before the local cost of living.</summary>
        public decimal MonthlyCost { get; }

        /// <summary>Stat changes per month.</summary>
        public IReadOnlyList<StatAmount> MonthlyStats { get; }
    }
}
