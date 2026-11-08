using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// One rung of a career ladder.
    /// </summary>
    public sealed class CareerLevelDefinition : IDefinition
    {
        /// <summary>
        /// Creates the level.
        /// </summary>
        /// <param name="id">Id unique within the track.</param>
        /// <param name="titleKey">Localization key of the job title.</param>
        /// <param name="monthlySalary">Real monthly salary (base income for volatile careers).</param>
        /// <param name="promotionAfterMonths">Months in the level before the first review, or null for the top level.</param>
        /// <param name="promotionPerformance">Performance needed for promotion.</param>
        public CareerLevelDefinition(string id, string titleKey, decimal monthlySalary, int? promotionAfterMonths, float promotionPerformance)
        {
            if (monthlySalary < 0m || (promotionAfterMonths.HasValue && promotionAfterMonths.Value < 1) || !StatBounds.IsInRange(promotionPerformance))
            {
                throw new ArgumentOutOfRangeException(nameof(monthlySalary), "Salary, months or performance out of range.");
            }
            Id = ContentId.Require(id, nameof(id));
            TitleKey = ContentId.RequireLocKey(titleKey, nameof(titleKey));
            MonthlySalary = monthlySalary;
            PromotionAfterMonths = promotionAfterMonths;
            PromotionPerformance = promotionPerformance;
        }

        /// <summary>Id within the track.</summary>
        public string Id { get; }

        /// <summary>Localization key of the job title.</summary>
        public string TitleKey { get; }

        /// <summary>Real monthly salary.</summary>
        public decimal MonthlySalary { get; }

        /// <summary>Months before the first promotion review, or null at the top.</summary>
        public int? PromotionAfterMonths { get; }

        /// <summary>Performance needed for promotion.</summary>
        public float PromotionPerformance { get; }
    }
}
