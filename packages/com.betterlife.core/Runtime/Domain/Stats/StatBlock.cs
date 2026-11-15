using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Holds the stored values of all non-derived stats and evaluates derived stats on read.
    /// Derived stats are never stored, so they always agree with their inputs.
    /// </summary>
    public sealed class StatBlock : IStatReader
    {
        private readonly StatCatalog _catalog;
        private readonly Dictionary<StatId, float> _stored = new Dictionary<StatId, float>();

        /// <summary>
        /// Creates a block with every stored stat at its default value.
        /// </summary>
        /// <param name="catalog">Stat catalog.</param>
        public StatBlock(StatCatalog catalog)
        {
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            foreach (StatDefinition definition in catalog.Definitions.Where(d => !catalog.IsDerived(d.Id)))
            {
                _stored.Add(definition.Id, definition.DefaultValue);
            }
        }

        /// <summary>The catalog this block is built from.</summary>
        public StatCatalog Catalog => _catalog;

        /// <summary>Stored (non-derived) values, for saving.</summary>
        public IReadOnlyDictionary<StatId, float> StoredValues => _stored;

        /// <inheritdoc />
        public float GetValue(StatId id)
        {
            if (_catalog.TryGetFormula(id, out IStatFormula formula))
            {
                return formula.Evaluate(this);
            }
            if (!_stored.TryGetValue(id, out float value))
            {
                throw new KeyNotFoundException($"Unknown stat id '{id}'.");
            }
            return value;
        }

        /// <summary>
        /// Sets a stored stat, clamped to the stat range. Derived stats cannot be set.
        /// </summary>
        /// <param name="id">Stored stat id.</param>
        /// <param name="value">New value.</param>
        /// <returns>The value actually stored.</returns>
        public float SetStored(StatId id, float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentException("Value must be finite.", nameof(value));
            }
            if (_catalog.IsDerived(id))
            {
                throw new InvalidOperationException($"Stat '{id}' is derived and cannot be set directly.");
            }
            if (!_stored.ContainsKey(id))
            {
                throw new KeyNotFoundException($"Unknown stat id '{id}'.");
            }

            float clamped = StatBounds.Clamp(value);
            _stored[id] = clamped;
            return clamped;
        }

        /// <summary>
        /// Explains a derived stat through its formula.
        /// </summary>
        /// <param name="id">Derived stat id.</param>
        /// <returns>The formula breakdown.</returns>
        public StatBreakdown ExplainFormula(StatId id)
        {
            if (!_catalog.TryGetFormula(id, out IStatFormula formula))
            {
                throw new InvalidOperationException($"Stat '{id}' is not derived.");
            }
            return formula.Explain(this, _catalog);
        }
    }
}
