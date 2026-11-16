using System;
using BetterLife.Core.Rules;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Raises or lowers a milestone's chance while a condition holds (usually: an earlier milestone happened).
    /// </summary>
    public sealed class MilestoneModifier
    {
        /// <summary>
        /// Creates a modifier.
        /// </summary>
        /// <param name="condition">When it applies.</param>
        /// <param name="add">Percentage points added to the chance (negative to lower it).</param>
        public MilestoneModifier(ICondition condition, float add)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Add = add;
        }

        /// <summary>When it applies.</summary>
        public ICondition Condition { get; }

        /// <summary>Percentage points added to the chance.</summary>
        public float Add { get; }
    }
}
