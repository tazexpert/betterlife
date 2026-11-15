using BetterLife.Core.Events;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Published after each month's finances are processed.
    /// </summary>
    public sealed class FinanceMonthReportEvent : IGameEvent
    {
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="report">The report.</param>
        public FinanceMonthReportEvent(FinanceMonthReport report)
        {
            Report = report;
        }

        /// <summary>The report.</summary>
        public FinanceMonthReport Report { get; }
    }
}
