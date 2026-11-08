using System;
using System.Collections.Generic;
using BetterLife.Core.Domain.Stats;

namespace BetterLife.Core.Domain.Genetics
{
    /// <summary>
    /// Inherited potential per heritable stat (0-100). Fixed at birth and passed on to children;
    /// the character's current stats may drift away from it over life.
    /// </summary>
    public sealed class Genome
    {
        private readonly Dictionary<StatId, float> _values = new Dictionary<StatId, float>();

        /// <summary>All values.</summary>
        public IReadOnlyDictionary<StatId, float> Values => _values;

        /// <summary>
        /// Returns the potential of a stat, or null when it is not heritable.
        /// </summary>
        /// <param name="statId">Stat id.</param>
        /// <returns>Value or null.</returns>
        public float? Get(StatId statId) => _values.TryGetValue(statId, out float value) ? value : (float?)null;

        /// <summary>
        /// Sets the potential of a stat, clamped to 0-100.
        /// </summary>
        /// <param name="statId">Stat id.</param>
        /// <param name="value">Finite value.</param>
        public void Set(StatId statId, float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentException("Value must be finite.", nameof(value));
            }
            _values[statId] = StatBounds.Clamp(value);
        }
    }
}
