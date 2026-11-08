using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Macro-economy content (content/economy.json). Phases must form a closed cycle of known ids.
    /// </summary>
    public sealed class EconomySettings
    {
        /// <summary>
        /// Creates settings.
        /// </summary>
        /// <param name="baseYear">Year whose prices all amounts are expressed in.</param>
        /// <param name="historicalInflation">Average annual inflation used to price earlier eras.</param>
        /// <param name="maxDebtBalance">Cap of any single loan balance; positive.</param>
        /// <param name="phases">Business-cycle phases.</param>
        public EconomySettings(int baseYear, float historicalInflation, decimal maxDebtBalance, IEnumerable<EconomyPhaseDefinition> phases)
        {
            if (historicalInflation <= -1f || float.IsNaN(historicalInflation) || float.IsInfinity(historicalInflation))
            {
                throw new ArgumentOutOfRangeException(nameof(historicalInflation), historicalInflation, "Inflation must exceed -100%.");
            }
            if (maxDebtBalance <= 0m)
            {
                throw new ArgumentOutOfRangeException(nameof(maxDebtBalance), maxDebtBalance, "Cap must be positive.");
            }
            BaseYear = baseYear;
            HistoricalInflation = historicalInflation;
            MaxDebtBalance = maxDebtBalance;
            Phases = new DefinitionCatalog<EconomyPhaseDefinition>(phases ?? throw new ArgumentNullException(nameof(phases)));
            if (Phases.All.Count == 0)
            {
                throw new ArgumentException("At least one phase is required.", nameof(phases));
            }
            foreach (EconomyPhaseDefinition phase in Phases.All.Where(p => !Phases.Contains(p.NextId)))
            {
                throw new ArgumentException($"Phase '{phase.Id}' is followed by unknown phase '{phase.NextId}'.", nameof(phases));
            }
        }

        /// <summary>Year whose prices all amounts are expressed in.</summary>
        public int BaseYear { get; }

        /// <summary>Average annual inflation before the game's own history.</summary>
        public float HistoricalInflation { get; }

        /// <summary>Cap of any single loan balance.</summary>
        public decimal MaxDebtBalance { get; }

        /// <summary>Business-cycle phases.</summary>
        public DefinitionCatalog<EconomyPhaseDefinition> Phases { get; }
    }
}
