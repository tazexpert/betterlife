using System;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// How well two people fit.
    /// </summary>
    public sealed class CompatibilityRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="baseScore">Starting score.</param>
        /// <param name="sharedTrait">Points per shared trait.</param>
        /// <param name="conflictingTrait">Points per conflicting trait (same exclusive group).</param>
        /// <param name="perYearAgeGap">Points per year of age difference.</param>
        /// <param name="looksWeight">Weight of the NPC's looks above 50.</param>
        /// <param name="charismaWeight">Weight of the NPC's charisma above 50.</param>
        public CompatibilityRules(float baseScore, float sharedTrait, float conflictingTrait, float perYearAgeGap, float looksWeight, float charismaWeight)
        {
            BaseScore = baseScore;
            SharedTrait = sharedTrait;
            ConflictingTrait = conflictingTrait;
            PerYearAgeGap = perYearAgeGap;
            LooksWeight = looksWeight;
            CharismaWeight = charismaWeight;
        }

        /// <summary>Starting score.</summary>
        public float BaseScore { get; }

        /// <summary>Points per shared trait.</summary>
        public float SharedTrait { get; }

        /// <summary>Points per conflicting trait (same exclusive group).</summary>
        public float ConflictingTrait { get; }

        /// <summary>Points per year of age difference.</summary>
        public float PerYearAgeGap { get; }

        /// <summary>Weight of the NPC's looks above 50.</summary>
        public float LooksWeight { get; }

        /// <summary>Weight of the NPC's charisma above 50.</summary>
        public float CharismaWeight { get; }
    }
}
