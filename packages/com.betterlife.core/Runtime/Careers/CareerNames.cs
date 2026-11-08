using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// JSON names of the career enums.
    /// </summary>
    public static class CareerNames
    {
        /// <summary>Categories by JSON name.</summary>
        public static readonly IReadOnlyDictionary<string, CareerCategory> Categories = new Dictionary<string, CareerCategory>
        {
            ["corporate"] = CareerCategory.Corporate,
            ["government"] = CareerCategory.Government,
            ["freelance"] = CareerCategory.Freelance,
            ["business"] = CareerCategory.Business,
            ["high_risk"] = CareerCategory.HighRisk,
            ["illegal"] = CareerCategory.Illegal,
            ["labor"] = CareerCategory.Labor,
        };

        /// <summary>
        /// Returns the JSON name of a category.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <returns>The name.</returns>
        public static string NameOf(CareerCategory category) => Categories.First(p => p.Value == category).Key;
    }
}
