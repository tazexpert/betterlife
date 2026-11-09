using System.Collections.Generic;

namespace BetterLife.Core.Domain
{
    /// <summary>
    /// JSON names of <see cref="Severity"/>.
    /// </summary>
    public static class SeverityNames
    {
        /// <summary>Severities by JSON name.</summary>
        public static readonly IReadOnlyDictionary<string, Severity> All = new Dictionary<string, Severity>
        {
            { "minor", Severity.Minor },
            { "normal", Severity.Normal },
            { "major", Severity.Major },
            { "critical", Severity.Critical },
        };
    }
}
