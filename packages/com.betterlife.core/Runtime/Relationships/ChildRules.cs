using System;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Fertility, pregnancy and the cost of raising children.
    /// </summary>
    public sealed class ChildRules
    {
        /// <summary>
        /// Creates the rules.
        /// </summary>
        /// <param name="fertileMinAge">Youngest parent age.</param>
        /// <param name="fertileMaxAge">Oldest parent age.</param>
        /// <param name="pregnancyWeeks">Weeks from conception to birth.</param>
        /// <param name="monthlyCost">Real monthly cost per child.</param>
        /// <param name="costUntilAge">Children cost money until this age.</param>
        public ChildRules(int fertileMinAge, int fertileMaxAge, int pregnancyWeeks, decimal monthlyCost, int costUntilAge)
        {
            if (fertileMinAge < 0 || fertileMaxAge < fertileMinAge || pregnancyWeeks < 1 || monthlyCost < 0m || costUntilAge < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fertileMinAge), "Values are out of range.");
            }
            FertileMinAge = fertileMinAge;
            FertileMaxAge = fertileMaxAge;
            PregnancyWeeks = pregnancyWeeks;
            MonthlyCost = monthlyCost;
            CostUntilAge = costUntilAge;
        }

        /// <summary>Youngest parent age.</summary>
        public int FertileMinAge { get; }

        /// <summary>Oldest parent age.</summary>
        public int FertileMaxAge { get; }

        /// <summary>Weeks from conception to birth.</summary>
        public int PregnancyWeeks { get; }

        /// <summary>Real monthly cost per child.</summary>
        public decimal MonthlyCost { get; }

        /// <summary>Children cost money until this age.</summary>
        public int CostUntilAge { get; }
    }
}
