using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Death risk tuning from content/mortality.json.
    /// </summary>
    public sealed class MortalitySettings
    {
        /// <summary>
        /// Creates settings.
        /// </summary>
        /// <param name="referenceAge">Age at which the Gompertz baseline is given; non-negative.</param>
        /// <param name="annualAtReference">Yearly death probability at the reference age, 0-1.</param>
        /// <param name="growthPerYear">Exponential growth of the baseline per year of age; non-negative.</param>
        /// <param name="maxAge">Age at which death is certain; positive.</param>
        /// <param name="healthAtZero">Risk multiplier at Health 0 (Health 50 is ×1); positive.</param>
        /// <param name="healthAtFull">Risk multiplier at Health 100; positive.</param>
        /// <param name="healthcareAtZero">Risk multiplier at healthcare quality 0 (50 is ×1); positive.</param>
        /// <param name="healthcareAtFull">Risk multiplier at healthcare quality 100; positive.</param>
        /// <param name="maxWeeklyProbability">Cap of the weekly probability below the maximum age, 0-1.</param>
        public MortalitySettings(int referenceAge, float annualAtReference, float growthPerYear, int maxAge,
            float healthAtZero, float healthAtFull, float healthcareAtZero, float healthcareAtFull, float maxWeeklyProbability)
        {
            if (referenceAge < 0 || maxAge <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxAge), maxAge, "Ages must be positive.");
            }
            RequireProbability(annualAtReference, nameof(annualAtReference));
            RequireProbability(maxWeeklyProbability, nameof(maxWeeklyProbability));
            if (!(growthPerYear >= 0f) || float.IsInfinity(growthPerYear))
            {
                throw new ArgumentOutOfRangeException(nameof(growthPerYear), growthPerYear, "Growth must be non-negative.");
            }
            foreach (float multiplier in new[] { healthAtZero, healthAtFull, healthcareAtZero, healthcareAtFull })
            {
                if (!(multiplier > 0f) || float.IsInfinity(multiplier))
                {
                    throw new ArgumentOutOfRangeException(nameof(healthAtZero), multiplier, "Multipliers must be positive.");
                }
            }
            ReferenceAge = referenceAge;
            AnnualAtReference = annualAtReference;
            GrowthPerYear = growthPerYear;
            MaxAge = maxAge;
            HealthAtZero = healthAtZero;
            HealthAtFull = healthAtFull;
            HealthcareAtZero = healthcareAtZero;
            HealthcareAtFull = healthcareAtFull;
            MaxWeeklyProbability = maxWeeklyProbability;
        }

        /// <summary>Age of the Gompertz baseline.</summary>
        public int ReferenceAge { get; }

        /// <summary>Yearly probability at the reference age.</summary>
        public float AnnualAtReference { get; }

        /// <summary>Growth per year of age.</summary>
        public float GrowthPerYear { get; }

        /// <summary>Age at which death is certain.</summary>
        public int MaxAge { get; }

        /// <summary>Multiplier at Health 0.</summary>
        public float HealthAtZero { get; }

        /// <summary>Multiplier at Health 100.</summary>
        public float HealthAtFull { get; }

        /// <summary>Multiplier at healthcare 0.</summary>
        public float HealthcareAtZero { get; }

        /// <summary>Multiplier at healthcare 100.</summary>
        public float HealthcareAtFull { get; }

        /// <summary>Weekly probability cap.</summary>
        public float MaxWeeklyProbability { get; }

        /// <summary>Value at which a 0-100 factor leaves the risk unchanged.</summary>
        public const float Neutral = StatBounds.Max / 2f;

        private static void RequireProbability(float value, string name)
        {
            if (!(value >= 0f) || value > 1f)
            {
                throw new ArgumentOutOfRangeException(name, value, "Probability must be within 0-1.");
            }
        }
    }
}
