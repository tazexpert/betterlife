using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Rules;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Base weight times every modifier whose condition holds.
    /// </summary>
    public sealed class EventWeight
    {
        /// <summary>
        /// Creates a weight.
        /// </summary>
        /// <param name="baseWeight">Non-negative base weight.</param>
        /// <param name="modifiers">Conditional multipliers.</param>
        public EventWeight(float baseWeight, IEnumerable<WeightModifier> modifiers)
        {
            if (!(baseWeight >= 0f) || float.IsInfinity(baseWeight))
            {
                throw new ArgumentOutOfRangeException(nameof(baseWeight), baseWeight, "Base weight must be non-negative.");
            }
            Base = baseWeight;
            Modifiers = (modifiers ?? Enumerable.Empty<WeightModifier>()).ToList();
        }

        /// <summary>Base weight.</summary>
        public float Base { get; }

        /// <summary>Conditional multipliers.</summary>
        public IReadOnlyList<WeightModifier> Modifiers { get; }

        /// <summary>
        /// Returns the weight in the current context.
        /// </summary>
        /// <param name="context">Rule context.</param>
        /// <returns>Non-negative weight.</returns>
        public float Evaluate(RuleContext context)
        {
            float weight = Base;
            foreach (WeightModifier modifier in Modifiers)
            {
                if (modifier.Condition.Evaluate(context))
                {
                    weight *= modifier.Multiplier;
                }
            }
            return weight;
        }
    }
}
