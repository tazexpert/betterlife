using System.Collections.Generic;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Economy;
using BetterLife.Core.Time;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Pays the monthly pension of a retired character.
    /// </summary>
    public sealed class PensionIncomeSource : IIncomeSource
    {
        /// <summary>Source key of the pension.</summary>
        public const string SourceKey = "career.pension";

        /// <inheritdoc />
        public IEnumerable<IncomeItem> MonthlyIncome(Character character, GameDate week)
        {
            if (character != null && character.Career.IsRetired && character.Career.PensionMonthly > 0m)
            {
                yield return new IncomeItem(SourceKey, character.Career.PensionMonthly, true);
            }
        }
    }
}
