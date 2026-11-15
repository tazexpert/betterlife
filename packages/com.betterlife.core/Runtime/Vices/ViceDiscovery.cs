using System;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// When family find out about a habit.
    /// </summary>
    public sealed class ViceDiscovery
    {
        /// <summary>
        /// Creates the value.
        /// </summary>
        /// <param name="fromAddiction">Addiction from which family may notice.</param>
        /// <param name="weeklyChance">Chance per week, 0-1.</param>
        /// <param name="effects">Effects applied when they find out.</param>
        public ViceDiscovery(float fromAddiction, float weeklyChance, EffectSequence effects)
        {
            if (!(weeklyChance >= 0f) || weeklyChance > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(fromAddiction), "Values are out of range.");
            }
            FromAddiction = fromAddiction;
            WeeklyChance = weeklyChance;
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }

        /// <summary>Addiction from which family may notice.</summary>
        public float FromAddiction { get; }

        /// <summary>Chance per week, 0-1.</summary>
        public float WeeklyChance { get; }

        /// <summary>Effects applied when they find out.</summary>
        public EffectSequence Effects { get; }
    }
}
