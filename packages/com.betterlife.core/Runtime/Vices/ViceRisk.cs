using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Vices
{
    /// <summary>
    /// A chance per use that something bad happens (an accident, an arrest...).
    /// </summary>
    public sealed class ViceRisk
    {
        /// <summary>
        /// Creates the value.
        /// </summary>
        /// <param name="chance">Chance per use, 0-1.</param>
        /// <param name="reasonKey">Localization key of what happened.</param>
        /// <param name="severity">How hard it interrupts time.</param>
        /// <param name="effects">Effects applied when it happens.</param>
        public ViceRisk(float chance, string reasonKey, Severity severity, EffectSequence effects)
        {
            if (!(chance >= 0f) || chance > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(chance), "Values are out of range.");
            }
            Chance = chance;
            ReasonKey = reasonKey ?? throw new ArgumentNullException(nameof(reasonKey));
            Severity = severity;
            Effects = effects ?? throw new ArgumentNullException(nameof(effects));
        }

        /// <summary>Chance per use, 0-1.</summary>
        public float Chance { get; }

        /// <summary>Localization key of what happened.</summary>
        public string ReasonKey { get; }

        /// <summary>How hard it interrupts time.</summary>
        public Severity Severity { get; }

        /// <summary>Effects applied when it happens.</summary>
        public EffectSequence Effects { get; }
    }
}
