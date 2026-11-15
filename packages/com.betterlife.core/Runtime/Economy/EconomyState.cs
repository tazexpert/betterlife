using System;
using BetterLife.Core.Domain;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// The macro economy of a life: cycle phase, this year's inflation and policy rate, and the price level.
    /// </summary>
    public sealed class EconomyState
    {
        /// <summary>
        /// Creates a state.
        /// </summary>
        /// <param name="phaseId">Current phase id.</param>
        /// <param name="yearsLeftInPhase">Years until the next phase; at least 1.</param>
        /// <param name="annualInflation">This year's inflation.</param>
        /// <param name="policyRate">This year's policy rate.</param>
        /// <param name="priceLevel">Nominal baht per base-year baht; positive.</param>
        public EconomyState(string phaseId, int yearsLeftInPhase, float annualInflation, float policyRate, decimal priceLevel)
        {
            Set(phaseId, yearsLeftInPhase, annualInflation, policyRate, priceLevel);
        }

        /// <summary>Current phase id.</summary>
        public string PhaseId { get; private set; }

        /// <summary>Years until the next phase.</summary>
        public int YearsLeftInPhase { get; private set; }

        /// <summary>This year's inflation.</summary>
        public float AnnualInflation { get; private set; }

        /// <summary>This year's policy rate.</summary>
        public float PolicyRate { get; private set; }

        /// <summary>Nominal baht per base-year baht (prices of the current era).</summary>
        public decimal PriceLevel { get; private set; }

        /// <summary>
        /// Converts a real base-year amount to the nominal price of the current era (for display).
        /// </summary>
        /// <param name="real">Real amount.</param>
        /// <returns>Nominal amount.</returns>
        public decimal Nominal(decimal real) => Money.Round(real * PriceLevel);

        internal void Set(string phaseId, int yearsLeftInPhase, float annualInflation, float policyRate, decimal priceLevel)
        {
            if (yearsLeftInPhase < 1 || priceLevel <= 0m || annualInflation <= -1f || policyRate < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(priceLevel), "Economy values are out of range.");
            }
            PhaseId = ContentId.Require(phaseId, nameof(phaseId));
            YearsLeftInPhase = yearsLeftInPhase;
            AnnualInflation = annualInflation;
            PolicyRate = policyRate;
            PriceLevel = priceLevel;
        }
    }
}
