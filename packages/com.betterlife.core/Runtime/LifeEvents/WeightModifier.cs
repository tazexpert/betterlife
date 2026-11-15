using System;
using BetterLife.Core.Rules;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Multiplies an event's weight while a condition holds.
    /// </summary>
    public sealed class WeightModifier
    {
        /// <summary>
        /// Creates a modifier.
        /// </summary>
        /// <param name="condition">When the modifier applies.</param>
        /// <param name="multiplier">Non-negative multiplier (0 disables the event).</param>
        public WeightModifier(ICondition condition, float multiplier)
        {
            if (!(multiplier >= 0f) || float.IsInfinity(multiplier))
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, "Multiplier must be non-negative.");
            }
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Multiplier = multiplier;
        }

        /// <summary>When the modifier applies.</summary>
        public ICondition Condition { get; }

        /// <summary>Multiplier.</summary>
        public float Multiplier { get; }
    }
}
