using System;
using System.Collections.Generic;

namespace BetterLife.Core.Rules.Effects
{
    /// <summary>
    /// Applies one of two effect sequences depending on a condition evaluated at apply time,
    /// so outcomes can depend on the character's past (butterfly effect).
    /// </summary>
    public sealed class IfEffect : IEffect
    {
        /// <summary>
        /// Creates the effect.
        /// </summary>
        /// <param name="condition">Branch condition.</param>
        /// <param name="then">Effects when the condition passes.</param>
        /// <param name="otherwise">Effects when it fails; null for none.</param>
        public IfEffect(ICondition condition, EffectSequence then, EffectSequence otherwise = null)
        {
            Condition = condition ?? throw new ArgumentNullException(nameof(condition));
            Then = then ?? throw new ArgumentNullException(nameof(then));
            Else = otherwise ?? EffectSequence.Empty;
        }

        /// <summary>Branch condition.</summary>
        public ICondition Condition { get; }

        /// <summary>Effects when the condition passes.</summary>
        public EffectSequence Then { get; }

        /// <summary>Effects when it fails.</summary>
        public EffectSequence Else { get; }

        /// <inheritdoc />
        public EffectResult Apply(RuleContext context)
        {
            return Branch(context).Apply(context);
        }

        /// <inheritdoc />
        public IReadOnlyList<RuleText> Preview(RuleContext context)
        {
            return Branch(context).Preview(context);
        }

        private EffectSequence Branch(RuleContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            return Condition.Evaluate(context) ? Then : Else;
        }
    }
}
