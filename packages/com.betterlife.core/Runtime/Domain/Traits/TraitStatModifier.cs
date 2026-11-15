using System;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Domain.Traits
{
    /// <summary>
    /// Scales changes to one stat, for example "stress gains are 25% larger".
    /// </summary>
    public sealed class TraitStatModifier
    {
        /// <summary>
        /// Creates a modifier.
        /// </summary>
        /// <param name="statId">Affected stat.</param>
        /// <param name="direction">Which changes are scaled.</param>
        /// <param name="multiplier">Positive multiplier.</param>
        public TraitStatModifier(StatId statId, ModifierDirection direction, float multiplier)
        {
            if (float.IsNaN(multiplier) || float.IsInfinity(multiplier) || multiplier <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, "Multiplier must be positive and finite.");
            }

            StatId = statId;
            Direction = direction;
            Multiplier = multiplier;
        }

        /// <summary>Affected stat.</summary>
        public StatId StatId { get; }

        /// <summary>Which changes are scaled.</summary>
        public ModifierDirection Direction { get; }

        /// <summary>Multiplier.</summary>
        public float Multiplier { get; }

        /// <summary>
        /// Returns true when this modifier scales the given change.
        /// </summary>
        /// <param name="statId">Changed stat.</param>
        /// <param name="delta">Requested delta.</param>
        /// <returns>True when applicable.</returns>
        public bool AppliesTo(StatId statId, float delta)
        {
            if (statId != StatId || delta == 0f)
            {
                return false;
            }
            switch (Direction)
            {
                case ModifierDirection.Gain:
                    return delta > 0f;
                case ModifierDirection.Loss:
                    return delta < 0f;
                default:
                    return true;
            }
        }
    }
}
