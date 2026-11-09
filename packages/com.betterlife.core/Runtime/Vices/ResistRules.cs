using System;
using BetterLife.Core.Luck;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// How the character resists temptation.
    /// </summary>
    public sealed class ResistRules
    {
        /// <summary>
        /// Creates the value.
        /// </summary>
        /// <param name="chance">Base chance spec (willpower and karma).</param>
        /// <param name="perAddiction">Chance points per addiction point (negative).</param>
        /// <param name="willpowerGain">Willpower gained per resisted temptation.</param>
        public ResistRules(ChanceSpec chance, float perAddiction, float willpowerGain)
        {
            if (willpowerGain < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(chance), "Values are out of range.");
            }
            Chance = chance ?? throw new ArgumentNullException(nameof(chance));
            PerAddiction = perAddiction;
            WillpowerGain = willpowerGain;
        }

        /// <summary>Base chance spec (willpower and karma).</summary>
        public ChanceSpec Chance { get; }

        /// <summary>Chance points per addiction point (negative).</summary>
        public float PerAddiction { get; }

        /// <summary>Willpower gained per resisted temptation.</summary>
        public float WillpowerGain { get; }
    }
}
