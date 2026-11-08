using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Rules
{
    /// <summary>
    /// An ordered list of effects applied one after another.
    /// </summary>
    public sealed class EffectSequence
    {
        /// <summary>A sequence with no effects.</summary>
        public static readonly EffectSequence Empty = new EffectSequence(new IEffect[0]);

        /// <summary>
        /// Creates a sequence.
        /// </summary>
        /// <param name="effects">Effects in order.</param>
        public EffectSequence(IEnumerable<IEffect> effects)
        {
            if (effects == null)
            {
                throw new ArgumentNullException(nameof(effects));
            }
            Effects = effects.ToList();
            if (Effects.Any(e => e == null))
            {
                throw new ArgumentException("Effects must not contain null.", nameof(effects));
            }
        }

        /// <summary>Effects in order.</summary>
        public IReadOnlyList<IEffect> Effects { get; }

        /// <summary>
        /// Applies every effect in order.
        /// </summary>
        /// <param name="context">Rule context.</param>
        /// <returns>The combined result.</returns>
        public EffectResult Apply(RuleContext context)
        {
            return EffectResult.Combine(Effects.Select(e => e.Apply(context)).ToList());
        }

        /// <summary>
        /// Previews every effect in order.
        /// </summary>
        /// <param name="context">Rule context.</param>
        /// <returns>Player-facing lines.</returns>
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            return Effects.SelectMany(e => e.Preview(context)).ToList();
        }
    }
}
