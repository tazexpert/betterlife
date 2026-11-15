using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Luck;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// The outcome of reporting a scam.
    /// </summary>
    public sealed class ScamReportResult
    {
        /// <summary>No such incident.</summary>
        public const string ReasonUnknown = "scam.report.unknown";
        /// <summary>Already reported.</summary>
        public const string ReasonReported = "scam.report.already";
        /// <summary>Too late to report.</summary>
        public const string ReasonTooLate = "scam.report.too_late";

        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="reasonKeys">Why it could not be reported; empty when it was.</param>
        /// <param name="roll">Recovery roll, or null.</param>
        /// <param name="recovered">Money returned.</param>
        public ScamReportResult(IEnumerable<string> reasonKeys, ChanceRoll roll, decimal recovered)
        {
            ReasonKeys = (reasonKeys ?? Enumerable.Empty<string>()).ToList();
            Roll = roll;
            Recovered = recovered;
        }

        /// <summary>True when the report was made.</summary>
        public bool Reported => ReasonKeys.Count == 0;

        /// <summary>Why not.</summary>
        public IReadOnlyList<string> ReasonKeys { get; }

        /// <summary>Recovery roll, or null.</summary>
        public ChanceRoll Roll { get; }

        /// <summary>Money returned.</summary>
        public decimal Recovered { get; }
    }
}
