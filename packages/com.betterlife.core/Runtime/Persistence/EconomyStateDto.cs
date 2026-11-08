using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of the macro economy.</summary>
    [Serializable]
    public sealed class EconomyStateDto
    {
        /// <summary>Current phase id.</summary>
        public string PhaseId;

        /// <summary>Years left in the phase.</summary>
        public int YearsLeftInPhase;

        /// <summary>This year's inflation.</summary>
        public float AnnualInflation;

        /// <summary>This year's policy rate.</summary>
        public float PolicyRate;

        /// <summary>Price level as an invariant-culture decimal string.</summary>
        public string PriceLevel;
    }
}
