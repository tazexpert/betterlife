namespace BetterLife.Core.ViewModels
{
    /// <summary>
    /// One time the character was scammed, with the report option.
    /// </summary>
    public sealed class ScamIncidentRow
    {
        /// <summary>
        /// Creates a row.
        /// </summary>
        /// <param name="incidentId">Incident id.</param>
        /// <param name="title">Scam name.</param>
        /// <param name="detail">Date, loss and recovery.</param>
        /// <param name="canReport">Whether it can still be reported.</param>
        /// <param name="reportText">Report button text with the estimated chance, or null.</param>
        public ScamIncidentRow(int incidentId, string title, string detail, bool canReport, string reportText)
        {
            IncidentId = incidentId;
            Title = title;
            Detail = detail;
            CanReport = canReport;
            ReportText = reportText;
        }

        /// <summary>Incident id.</summary>
        public int IncidentId { get; }

        /// <summary>Scam name.</summary>
        public string Title { get; }

        /// <summary>Date, loss and recovery.</summary>
        public string Detail { get; }

        /// <summary>Whether it can still be reported.</summary>
        public bool CanReport { get; }

        /// <summary>Report button text, or null.</summary>
        public string ReportText { get; }
    }
}
