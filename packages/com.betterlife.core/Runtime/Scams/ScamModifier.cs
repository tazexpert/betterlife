using System;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Scams
{
    /// <summary>
    /// A targeting multiplier that applies while a condition on the character holds (in debt, unemployed...).
    /// </summary>
    public sealed class ScamModifier
    {
        /// <summary>
        /// Creates the value.
        /// </summary>
        /// <param name="condition">Condition.</param>
        /// <param name="multiplier">Multiplier while it holds.</param>
        public ScamModifier(ICondition condition, float multiplier)
        {
            if (multiplier < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(condition), "Values are out of range.");
            }
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Multiplier = multiplier;
        }

        /// <summary>Condition.</summary>
        public ICondition Condition { get; }

        /// <summary>Multiplier while it holds.</summary>
        public float Multiplier { get; }
    }
}
