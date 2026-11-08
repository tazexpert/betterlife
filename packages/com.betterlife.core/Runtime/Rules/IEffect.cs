using System.Collections.Generic;

namespace BetterLife.Core.Rules
{
    /// <summary>
    /// A data-driven change to the character. Effects return what happened and never publish events themselves;
    /// the calling system publishes.
    /// </summary>
    public interface IEffect
    {
        /// <summary>
        /// Applies the change.
        /// </summary>
        /// <param name="context">Rule context.</param>
        /// <returns>What happened.</returns>
        EffectResult Apply(RuleContext context);

        /// <summary>
        /// Describes the change before it happens (ranges are shown as ranges). Hidden changes return nothing.
        /// </summary>
        /// <param name="context">Rule context, used by conditional effects to pick a branch.</param>
        /// <returns>Player-facing lines.</returns>
        IReadOnlyList<RuleText> Preview(RuleContext context);
    }
}
