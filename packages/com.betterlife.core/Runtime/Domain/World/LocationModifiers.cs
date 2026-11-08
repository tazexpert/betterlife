using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Domain.World
{
    /// <summary>
    /// How a place affects life: cost of living (multiplier), education and healthcare quality (0-100), tax rate (0-1).
    /// City values override country values when present.
    /// </summary>
    public sealed class LocationModifiers
    {
        /// <summary>
        /// Creates modifiers; null fields inherit from the country.
        /// </summary>
        /// <param name="costOfLiving">Price multiplier; positive.</param>
        /// <param name="education">Education quality, 0-100.</param>
        /// <param name="healthcare">Healthcare quality, 0-100.</param>
        /// <param name="taxRate">Income tax rate, 0-1.</param>
        public LocationModifiers(float? costOfLiving, float? education, float? healthcare, float? taxRate)
        {
            if (costOfLiving.HasValue && (!(costOfLiving.Value > 0f) || float.IsInfinity(costOfLiving.Value)))
            {
                throw new ArgumentOutOfRangeException(nameof(costOfLiving), costOfLiving, "Cost of living must be positive.");
            }
            if (education.HasValue && !StatBounds.IsInRange(education.Value))
            {
                throw new ArgumentOutOfRangeException(nameof(education), education, "Education must be within 0-100.");
            }
            if (healthcare.HasValue && !StatBounds.IsInRange(healthcare.Value))
            {
                throw new ArgumentOutOfRangeException(nameof(healthcare), healthcare, "Healthcare must be within 0-100.");
            }
            if (taxRate.HasValue && (!(taxRate.Value >= 0f) || taxRate.Value > 1f))
            {
                throw new ArgumentOutOfRangeException(nameof(taxRate), taxRate, "Tax rate must be within 0-1.");
            }
            CostOfLiving = costOfLiving;
            Education = education;
            Healthcare = healthcare;
            TaxRate = taxRate;
        }

        /// <summary>Price multiplier, or null to inherit.</summary>
        public float? CostOfLiving { get; }

        /// <summary>Education quality, or null to inherit.</summary>
        public float? Education { get; }

        /// <summary>Healthcare quality, or null to inherit.</summary>
        public float? Healthcare { get; }

        /// <summary>Tax rate, or null to inherit.</summary>
        public float? TaxRate { get; }

        /// <summary>True when every field has a value.</summary>
        public bool IsComplete => CostOfLiving.HasValue && Education.HasValue && Healthcare.HasValue && TaxRate.HasValue;

        /// <summary>
        /// Returns these values with missing ones taken from <paramref name="fallback"/>.
        /// </summary>
        /// <param name="fallback">Values to inherit.</param>
        /// <returns>Combined modifiers.</returns>
        public LocationModifiers Over(LocationModifiers fallback)
        {
            if (fallback == null)
            {
                return this;
            }
            return new LocationModifiers(
                CostOfLiving ?? fallback.CostOfLiving,
                Education ?? fallback.Education,
                Healthcare ?? fallback.Healthcare,
                TaxRate ?? fallback.TaxRate);
        }
    }
}
