using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Time;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Everything that happened to the character's money in one month.
    /// </summary>
    public sealed class FinanceMonthReport
    {
        /// <summary>
        /// Creates a report.
        /// </summary>
        /// <param name="week">First week of the month.</param>
        /// <param name="lines">Lines in processing order.</param>
        /// <param name="netWorth">Net worth after the month.</param>
        public FinanceMonthReport(GameDate week, IReadOnlyList<FinanceLine> lines, decimal netWorth)
        {
            Week = week;
            Lines = lines;
            NetWorth = netWorth;
        }

        /// <summary>First week of the month.</summary>
        public GameDate Week { get; }

        /// <summary>Lines in processing order.</summary>
        public IReadOnlyList<FinanceLine> Lines { get; }

        /// <summary>Net worth after the month.</summary>
        public decimal NetWorth { get; }

        /// <summary>Gross income of the month.</summary>
        public decimal GrossIncome => Lines.Where(l => l.Category == FinanceLineCategory.Income).Sum(l => l.Amount);

        /// <summary>
        /// Returns the total of a category.
        /// </summary>
        /// <param name="category">Category.</param>
        /// <returns>Signed total.</returns>
        public decimal Total(FinanceLineCategory category) => Lines.Where(l => l.Category == category).Sum(l => l.Amount);
    }
}
