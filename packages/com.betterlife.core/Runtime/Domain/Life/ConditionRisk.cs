using System;

namespace BetterLife.Core.Domain.Life
{
    /// <summary>
    /// Yearly chance of developing a health condition during a life stage.
    /// </summary>
    public sealed class ConditionRisk
    {
        /// <summary>
        /// Creates a risk.
        /// </summary>
        /// <param name="conditionId">Health condition id.</param>
        /// <param name="annualChance">Chance per birthday, 0-1, at Health 50 (worse health raises it).</param>
        public ConditionRisk(string conditionId, float annualChance)
        {
            if (!(annualChance >= 0f) || annualChance > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(annualChance), annualChance, "Chance must be within 0-1.");
            }
            ConditionId = ContentId.Require(conditionId, nameof(conditionId));
            AnnualChance = annualChance;
        }

        /// <summary>Health condition id.</summary>
        public string ConditionId { get; }

        /// <summary>Chance per birthday at Health 50.</summary>
        public float AnnualChance { get; }
    }
}
