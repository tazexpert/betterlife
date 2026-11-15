using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// Why a vice tempts the character this much: the base chance and every multiplier.
    /// </summary>
    public sealed class TemptationBreakdown
    {
        /// <summary>Line key of the stress multiplier.</summary>
        public const string StressKey = "vice.temptation.stress";
        /// <summary>Line key of the addiction multiplier.</summary>
        public const string AddictionKey = "vice.temptation.addiction";
        /// <summary>Line key of the bad company multiplier.</summary>
        public const string BadCompanyKey = "vice.temptation.bad_company";
        /// <summary>Line key of the laziness multiplier.</summary>
        public const string LazinessKey = "vice.temptation.laziness";
        /// <summary>Line key of the life stage multiplier.</summary>
        public const string LifeStageKey = "vice.temptation.life_stage";
        /// <summary>Line key of the character's traits.</summary>
        public const string TraitsKey = "vice.temptation.traits";
        /// <summary>Line key of never having tried it.</summary>
        public const string FirstTryKey = "vice.temptation.first_try";

        /// <summary>
        /// Creates the breakdown.
        /// </summary>
        /// <param name="baseWeekly">Base weekly chance.</param>
        /// <param name="multipliers">Multipliers (localization key, value).</param>
        /// <param name="cap">Highest weekly chance.</param>
        public TemptationBreakdown(float baseWeekly, IEnumerable<(string Key, float Multiplier)> multipliers, float cap)
        {
            BaseWeekly = baseWeekly;
            Multipliers = (multipliers ?? throw new ArgumentNullException(nameof(multipliers))).ToList();
            WeeklyChance = Math.Max(0f, Math.Min(cap, Multipliers.Aggregate(baseWeekly, (chance, m) => chance * m.Multiplier)));
        }

        /// <summary>Base weekly chance.</summary>
        public float BaseWeekly { get; }

        /// <summary>Multipliers.</summary>
        public IReadOnlyList<(string Key, float Multiplier)> Multipliers { get; }

        /// <summary>Final weekly chance, 0-1.</summary>
        public float WeeklyChance { get; }
    }
}
