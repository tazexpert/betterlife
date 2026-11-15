using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Legacy
{
    /// <summary>
    /// How an heir inherits: inheritance tax, and the share of reputation and karma passed down.
    /// </summary>
    public sealed class LegacySettings
    {
        /// <summary>
        /// Creates the settings.
        /// </summary>
        /// <param name="taxExemption">Estate value free of inheritance tax (real money).</param>
        /// <param name="taxRate">Tax rate above the exemption (0..1).</param>
        /// <param name="reputationShare">Share of each reputation passed to the heir (0..1).</param>
        /// <param name="karmaShare">Share of merit and sin passed to the heir (0..1).</param>
        /// <param name="heirEducationPerformance">Performance of the schooling an adult heir is credited with (0..100).</param>
        public LegacySettings(decimal taxExemption, decimal taxRate, float reputationShare, float karmaShare, float heirEducationPerformance)
        {
            if (taxExemption < 0m || taxRate < 0m || taxRate > 1m || reputationShare < 0f || reputationShare > 1f || karmaShare < 0f || karmaShare > 1f
                || !StatBounds.IsInRange(heirEducationPerformance))
            {
                throw new ArgumentOutOfRangeException(nameof(taxRate), "Legacy settings are out of range.");
            }
            TaxExemption = taxExemption;
            TaxRate = taxRate;
            ReputationShare = reputationShare;
            KarmaShare = karmaShare;
            HeirEducationPerformance = heirEducationPerformance;
        }

        /// <summary>Estate value free of inheritance tax (real money).</summary>
        public decimal TaxExemption { get; }

        /// <summary>Tax rate above the exemption.</summary>
        public decimal TaxRate { get; }

        /// <summary>Share of each reputation passed to the heir.</summary>
        public float ReputationShare { get; }

        /// <summary>Share of merit and sin passed to the heir.</summary>
        public float KarmaShare { get; }

        /// <summary>Performance of the schooling an adult heir is credited with.</summary>
        public float HeirEducationPerformance { get; }
    }
}
