using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Luck
{
    /// <summary>
    /// Chance tuning from content/luck.json.
    /// </summary>
    public sealed class LuckSettings
    {
        /// <summary>
        /// Creates settings.
        /// </summary>
        /// <param name="chanceMin">Lowest final chance in percent (0-100).</param>
        /// <param name="chanceMax">Highest final chance in percent (at least the minimum).</param>
        /// <param name="itemBonusCap">Maximum percent from items (non-negative).</param>
        /// <param name="fortuneNeutral">Fortune that adds nothing (0-100).</param>
        /// <param name="pointsPerFortune">Chance percent per fortune point above neutral (non-negative).</param>
        public LuckSettings(float chanceMin, float chanceMax, float itemBonusCap, float fortuneNeutral, float pointsPerFortune)
        {
            if (!StatBounds.IsInRange(chanceMin) || !StatBounds.IsInRange(chanceMax) || chanceMax < chanceMin)
            {
                throw new ArgumentOutOfRangeException(nameof(chanceMax), chanceMax, "Chance bounds must satisfy 0 <= min <= max <= 100.");
            }
            if (!(itemBonusCap >= 0f) || float.IsInfinity(itemBonusCap))
            {
                throw new ArgumentOutOfRangeException(nameof(itemBonusCap), itemBonusCap, "Item cap must be non-negative.");
            }
            if (!StatBounds.IsInRange(fortuneNeutral))
            {
                throw new ArgumentOutOfRangeException(nameof(fortuneNeutral), fortuneNeutral, "Neutral fortune must be within 0-100.");
            }
            if (!(pointsPerFortune >= 0f) || float.IsInfinity(pointsPerFortune))
            {
                throw new ArgumentOutOfRangeException(nameof(pointsPerFortune), pointsPerFortune, "Points must be non-negative.");
            }
            ChanceMin = chanceMin;
            ChanceMax = chanceMax;
            ItemBonusCap = itemBonusCap;
            FortuneNeutral = fortuneNeutral;
            PointsPerFortune = pointsPerFortune;
        }

        /// <summary>Lowest final chance in percent.</summary>
        public float ChanceMin { get; }

        /// <summary>Highest final chance in percent.</summary>
        public float ChanceMax { get; }

        /// <summary>Maximum percent from items.</summary>
        public float ItemBonusCap { get; }

        /// <summary>Fortune that adds nothing.</summary>
        public float FortuneNeutral { get; }

        /// <summary>Chance percent per fortune point above neutral.</summary>
        public float PointsPerFortune { get; }
    }
}
