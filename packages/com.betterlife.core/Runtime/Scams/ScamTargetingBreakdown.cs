using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// Why scammers target the character this often: the base chance and every multiplier.
    /// </summary>
    public sealed class ScamTargetingBreakdown
    {
        /// <summary>Line key of age.</summary>
        public const string AgeKey = "scam.targeting.age";
        /// <summary>Line key of loneliness.</summary>
        public const string LonelinessKey = "scam.targeting.loneliness";
        /// <summary>Line key of time online.</summary>
        public const string OnlineKey = "scam.targeting.online";
        /// <summary>Line key of money.</summary>
        public const string WealthKey = "scam.targeting.wealth";
        /// <summary>Line key of the character's situation (debt, unemployment...).</summary>
        public const string SituationKey = "scam.targeting.situation";

        /// <summary>
        /// Creates the breakdown.
        /// </summary>
        /// <param name="baseMonthly">Base monthly chance.</param>
        /// <param name="multipliers">Multipliers (localization key, value).</param>
        /// <param name="cap">Highest monthly chance.</param>
        public ScamTargetingBreakdown(float baseMonthly, IEnumerable<(string Key, float Multiplier)> multipliers, float cap)
        {
            BaseMonthly = baseMonthly;
            Multipliers = (multipliers ?? throw new ArgumentNullException(nameof(multipliers))).ToList();
            MonthlyChance = Math.Max(0f, Math.Min(cap, Multipliers.Aggregate(baseMonthly, (chance, m) => chance * m.Multiplier)));
        }

        /// <summary>Base monthly chance.</summary>
        public float BaseMonthly { get; }

        /// <summary>Multipliers.</summary>
        public IReadOnlyList<(string Key, float Multiplier)> Multipliers { get; }

        /// <summary>Final monthly chance, 0-1.</summary>
        public float MonthlyChance { get; }
    }
}
