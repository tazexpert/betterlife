using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// What happens after a choice succeeds or fails: a result text and effects.
    /// </summary>
    public sealed class EventOutcome
    {
        /// <summary>
        /// Creates an outcome.
        /// </summary>
        /// <param name="resultKey">Localization key of the result text.</param>
        /// <param name="effects">Effects applied in order.</param>
        public EventOutcome(string resultKey, EffectSequence effects)
        {
            ResultKey = ContentId.RequireLocKey(resultKey, nameof(resultKey));
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }

        /// <summary>Localization key of the result text.</summary>
        public string ResultKey { get; }

        /// <summary>Effects applied in order.</summary>
        public EffectSequence Effects { get; }
    }
}
