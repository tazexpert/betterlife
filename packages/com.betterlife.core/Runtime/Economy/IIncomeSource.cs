using System.Collections.Generic;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Time;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Something that pays the character each month (family allowance now; jobs and businesses in later tasks).
    /// </summary>
    public interface IIncomeSource
    {
        /// <summary>
        /// Returns this month's gross income lines (category Income).
        /// </summary>
        /// <param name="character">Character.</param>
        /// <param name="week">First week of the month.</param>
        /// <returns>Income lines; empty when nothing is paid.</returns>
        IEnumerable<IncomeItem> MonthlyIncome(Character character, GameDate week);
    }
}
