using System;
using System.Collections.Generic;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Time;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// The family's monthly allowance for a dependent child, by household income level.
    /// </summary>
    public sealed class FamilyAllowanceSource : IIncomeSource
    {
        /// <summary>Source key of the allowance.</summary>
        public const string SourceKey = "finance.family_allowance";

        private readonly FinanceSettings _settings;

        /// <summary>
        /// Creates the source.
        /// </summary>
        /// <param name="settings">Finance settings.</param>
        public FamilyAllowanceSource(FinanceSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <inheritdoc />
        public IEnumerable<IncomeItem> MonthlyIncome(Character character, GameDate week)
        {
            int age = character.AgeAt(week);
            if (character.Family == null || age < _settings.AllowanceStartAge || age >= _settings.IndependenceAge)
            {
                yield break;
            }
            int level = character.Catalogs.FamilyBackgrounds.Get(character.Family.BackgroundId).HouseholdIncomeLevel;
            if (_settings.FamilySupport.TryGetValue(level, out FamilySupportLevel support) && support.MonthlyAllowance > 0m)
            {
                yield return new IncomeItem(SourceKey, support.MonthlyAllowance, false);
            }
        }
    }
}
