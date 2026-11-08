using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Domain.Family
{
    /// <summary>
    /// A family's economic standing, loaded from content/family_backgrounds.json.
    /// </summary>
    public sealed class FamilyBackgroundDefinition : IDefinition
    {
        /// <summary>
        /// Creates a background.
        /// </summary>
        /// <param name="id">Background id.</param>
        /// <param name="nameKey">Localization key of the name.</param>
        /// <param name="weight">Random selection weight; non-negative.</param>
        /// <param name="householdIncomeLevel">Relative household income level (1 = lowest), used by the economy.</param>
        /// <param name="statModifiers">Stat changes at birth.</param>
        public FamilyBackgroundDefinition(string id, string nameKey, float weight, int householdIncomeLevel, IEnumerable<StatAmount> statModifiers)
        {
            if (!(weight >= 0f) || float.IsInfinity(weight))
            {
                throw new ArgumentOutOfRangeException(nameof(weight), weight, "Weight must be non-negative.");
            }
            if (householdIncomeLevel < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(householdIncomeLevel), householdIncomeLevel, "Income level starts at 1.");
            }
            Id = ContentId.Require(id, nameof(id));
            NameKey = ContentId.RequireLocKey(nameKey, nameof(nameKey));
            Weight = weight;
            HouseholdIncomeLevel = householdIncomeLevel;
            StatModifiers = (statModifiers ?? Enumerable.Empty<StatAmount>()).ToList();
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the name (also the source key of its stat changes).</summary>
        public string NameKey { get; }

        /// <summary>Random selection weight.</summary>
        public float Weight { get; }

        /// <summary>Relative household income level.</summary>
        public int HouseholdIncomeLevel { get; }

        /// <summary>Stat changes at birth.</summary>
        public IReadOnlyList<StatAmount> StatModifiers { get; }
    }
}
