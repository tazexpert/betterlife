using System.Collections.Generic;

namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// Everything the Money tab shows.
    /// </summary>
    public sealed class FinanceState
    {
        /// <summary>
        /// Creates the state.
        /// </summary>
        /// <param name="summary">Cash, investments, debts, net worth, income.</param>
        /// <param name="economy">Economy phase, inflation, interest rate.</param>
        /// <param name="holdings">Investments and property.</param>
        /// <param name="debts">Debts.</param>
        /// <param name="report">Last month's money in and out, by category.</param>
        /// <param name="reportNote">Shown instead of the report when there is none yet; empty otherwise.</param>
        /// <param name="lifestyles">Lifestyle tiers.</param>
        /// <param name="canChooseLifestyle">Whether the tier can be changed now.</param>
        /// <param name="scams">Scam incidents, newest first.</param>
        public FinanceState(IReadOnlyList<ValueRow> summary, IReadOnlyList<ValueRow> economy, IReadOnlyList<ValueRow> holdings, IReadOnlyList<ValueRow> debts,
            IReadOnlyList<ValueRow> report, string reportNote, IReadOnlyList<LifestyleOption> lifestyles, bool canChooseLifestyle,
            IReadOnlyList<ScamIncidentRow> scams)
        {
            Summary = summary;
            Economy = economy;
            Holdings = holdings;
            Debts = debts;
            Report = report;
            ReportNote = reportNote;
            Lifestyles = lifestyles;
            CanChooseLifestyle = canChooseLifestyle;
            Scams = scams;
        }

        /// <summary>Cash, investments, debts, net worth, income.</summary>
        public IReadOnlyList<ValueRow> Summary { get; }

        /// <summary>Economy phase, inflation, interest rate.</summary>
        public IReadOnlyList<ValueRow> Economy { get; }

        /// <summary>Investments and property.</summary>
        public IReadOnlyList<ValueRow> Holdings { get; }

        /// <summary>Debts.</summary>
        public IReadOnlyList<ValueRow> Debts { get; }

        /// <summary>Last month's money in and out, by category.</summary>
        public IReadOnlyList<ValueRow> Report { get; }

        /// <summary>Shown instead of the report when there is none yet.</summary>
        public string ReportNote { get; }

        /// <summary>Lifestyle tiers.</summary>
        public IReadOnlyList<LifestyleOption> Lifestyles { get; }

        /// <summary>Whether the tier can be changed now.</summary>
        public bool CanChooseLifestyle { get; }

        /// <summary>Scam incidents, newest first.</summary>
        public IReadOnlyList<ScamIncidentRow> Scams { get; }
    }
}
