using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Luck;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Whether the character may apply for a career, and the odds.
    /// </summary>
    public sealed class HiringQuote
    {
        /// <summary>Too young to work.</summary>
        public const string ReasonTooYoung = "career.apply.too_young";
        /// <summary>At or past retirement age.</summary>
        public const string ReasonTooOld = "career.apply.too_old";
        /// <summary>The track's requirement is not met.</summary>
        public const string ReasonRequirement = "career.apply.requirement";
        /// <summary>Applied to this track too recently.</summary>
        public const string ReasonRetryWait = "career.apply.retry_wait";
        /// <summary>Already working in this track.</summary>
        public const string ReasonSameJob = "career.apply.same_job";

        /// <summary>
        /// Creates the quote.
        /// </summary>
        /// <param name="reasonKeys">Why the character may not apply; empty when eligible.</param>
        /// <param name="prestigeAdjustment">Chance points from education prestige (included in the chance).</param>
        /// <param name="networkAdjustment">Chance points from close contacts (included in the chance).</param>
        /// <param name="chance">Hiring chance.</param>
        /// <param name="startingSalary">Real monthly salary of the entry level.</param>
        public HiringQuote(IReadOnlyList<string> reasonKeys, float prestigeAdjustment, float networkAdjustment, ChanceBreakdown chance, decimal startingSalary)
        {
            ReasonKeys = (reasonKeys ?? throw new ArgumentNullException(nameof(reasonKeys))).ToList();
            PrestigeAdjustment = prestigeAdjustment;
            NetworkAdjustment = networkAdjustment;
            Chance = chance ?? throw new ArgumentNullException(nameof(chance));
            StartingSalary = startingSalary;
        }

        /// <summary>True when the character may apply.</summary>
        public bool Eligible => ReasonKeys.Count == 0;

        /// <summary>Why not, as localization keys.</summary>
        public IReadOnlyList<string> ReasonKeys { get; }

        /// <summary>Chance points from education prestige.</summary>
        public float PrestigeAdjustment { get; }

        /// <summary>Chance points from close contacts.</summary>
        public float NetworkAdjustment { get; }

        /// <summary>Hiring chance.</summary>
        public ChanceBreakdown Chance { get; }

        /// <summary>Entry-level salary.</summary>
        public decimal StartingSalary { get; }
    }
}
