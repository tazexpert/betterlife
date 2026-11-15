using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// What one use of a vice did.
    /// </summary>
    public sealed class ViceUseResult
    {
        /// <summary>Too young.</summary>
        public const string ReasonTooYoung = "vice.use.too_young";
        /// <summary>Vice content is turned off.</summary>
        public const string ReasonDisabled = "vice.use.disabled";
        /// <summary>Not enough cash.</summary>
        public const string ReasonCash = "vice.use.cash";

        /// <summary>
        /// Creates the result.
        /// </summary>
        /// <param name="viceId">Vice id.</param>
        /// <param name="reasonKeys">Why it did not happen; empty when it did.</param>
        /// <param name="voluntary">True when the player chose it.</param>
        /// <param name="spent">Cash spent (or staked).</param>
        /// <param name="won">Gross winnings (gambling).</param>
        /// <param name="relapsed">True when it broke a quit attempt.</param>
        /// <param name="riskKeys">Risks that happened.</param>
        public ViceUseResult(string viceId, IEnumerable<string> reasonKeys, bool voluntary, decimal spent, decimal won, bool relapsed, IEnumerable<string> riskKeys)
        {
            ViceId = viceId;
            ReasonKeys = (reasonKeys ?? Enumerable.Empty<string>()).ToList();
            Voluntary = voluntary;
            Spent = spent;
            Won = won;
            Relapsed = relapsed;
            RiskKeys = (riskKeys ?? Enumerable.Empty<string>()).ToList();
        }

        /// <summary>Vice id.</summary>
        public string ViceId { get; }

        /// <summary>True when it happened.</summary>
        public bool Performed => ReasonKeys.Count == 0;

        /// <summary>Why it did not happen.</summary>
        public IReadOnlyList<string> ReasonKeys { get; }

        /// <summary>True when the player chose it.</summary>
        public bool Voluntary { get; }

        /// <summary>Cash spent or staked.</summary>
        public decimal Spent { get; }

        /// <summary>Gross winnings.</summary>
        public decimal Won { get; }

        /// <summary>True when it broke a quit attempt.</summary>
        public bool Relapsed { get; }

        /// <summary>Risks that happened.</summary>
        public IReadOnlyList<string> RiskKeys { get; }
    }
}
