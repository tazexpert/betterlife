namespace BetterLife.Core.Life
{
    /// <summary>
    /// How this year's death risk is composed (for balancing and an optional health screen).
    /// </summary>
    public sealed class MortalityBreakdown
    {
        /// <summary>
        /// Creates a breakdown.
        /// </summary>
        /// <param name="age">Age in years.</param>
        /// <param name="ageRisk">Gompertz yearly probability.</param>
        /// <param name="stageRisk">Extra yearly probability of the life stage.</param>
        /// <param name="conditionRisk">Yearly probability added by health conditions.</param>
        /// <param name="healthMultiplier">Multiplier from Health.</param>
        /// <param name="healthcareMultiplier">Multiplier from local healthcare.</param>
        /// <param name="annual">Final yearly probability.</param>
        /// <param name="weekly">Probability for this week.</param>
        public MortalityBreakdown(int age, float ageRisk, float stageRisk, float conditionRisk, float healthMultiplier, float healthcareMultiplier, float annual, float weekly)
        {
            Age = age;
            AgeRisk = ageRisk;
            StageRisk = stageRisk;
            ConditionRisk = conditionRisk;
            HealthMultiplier = healthMultiplier;
            HealthcareMultiplier = healthcareMultiplier;
            Annual = annual;
            Weekly = weekly;
        }

        /// <summary>Age in years.</summary>
        public int Age { get; }

        /// <summary>Gompertz yearly probability.</summary>
        public float AgeRisk { get; }

        /// <summary>Extra yearly probability of the stage.</summary>
        public float StageRisk { get; }

        /// <summary>Yearly probability from conditions.</summary>
        public float ConditionRisk { get; }

        /// <summary>Multiplier from Health.</summary>
        public float HealthMultiplier { get; }

        /// <summary>Multiplier from healthcare.</summary>
        public float HealthcareMultiplier { get; }

        /// <summary>Final yearly probability.</summary>
        public float Annual { get; }

        /// <summary>Probability for this week.</summary>
        public float Weekly { get; }
    }
}
