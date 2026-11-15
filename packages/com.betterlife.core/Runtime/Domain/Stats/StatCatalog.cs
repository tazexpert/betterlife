using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// All stat definitions plus the formulas of derived stats, validated for consistency at construction.
    /// </summary>
    public sealed class StatCatalog
    {
        private readonly Dictionary<StatId, StatDefinition> _definitions = new Dictionary<StatId, StatDefinition>();
        private readonly Dictionary<StatId, IStatFormula> _formulas = new Dictionary<StatId, IStatFormula>();
        private readonly List<StatDefinition> _ordered;

        /// <summary>
        /// Creates a catalog. Throws when ids repeat, references are missing, formulas target non-core stats,
        /// or a formula reads another derived stat.
        /// </summary>
        /// <param name="definitions">Stat definitions.</param>
        /// <param name="formulas">Formulas of derived stats.</param>
        public StatCatalog(IEnumerable<StatDefinition> definitions, IEnumerable<IStatFormula> formulas)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }
            if (formulas == null)
            {
                throw new ArgumentNullException(nameof(formulas));
            }

            _ordered = definitions.ToList();
            foreach (StatDefinition definition in _ordered)
            {
                if (definition == null)
                {
                    throw new ArgumentException("Definitions must not contain null.", nameof(definitions));
                }
                if (_definitions.ContainsKey(definition.Id))
                {
                    throw new ArgumentException($"Duplicate stat id '{definition.Id}'.", nameof(definitions));
                }
                _definitions.Add(definition.Id, definition);
            }

            foreach (StatDefinition definition in _ordered)
            {
                ValidateParent(definition);
            }

            foreach (IStatFormula formula in formulas)
            {
                AddFormula(formula);
            }
        }

        /// <summary>All definitions in declaration order.</summary>
        public IReadOnlyList<StatDefinition> Definitions => _ordered;

        /// <summary>Ids of the core (dashboard) stats in declaration order.</summary>
        public IEnumerable<StatId> CoreStatIds => _ordered.Where(d => d.Layer == StatLayer.Core).Select(d => d.Id);

        /// <summary>
        /// Returns true when the stat exists.
        /// </summary>
        /// <param name="id">Stat id.</param>
        /// <returns>True when defined.</returns>
        public bool Contains(StatId id) => _definitions.ContainsKey(id);

        /// <summary>
        /// Returns a definition or throws when unknown.
        /// </summary>
        /// <param name="id">Stat id.</param>
        /// <returns>The definition.</returns>
        public StatDefinition GetDefinition(StatId id)
        {
            if (!_definitions.TryGetValue(id, out StatDefinition definition))
            {
                throw new KeyNotFoundException($"Unknown stat id '{id}'.");
            }
            return definition;
        }

        /// <summary>
        /// Returns true when the stat is computed by a formula rather than stored.
        /// </summary>
        /// <param name="id">Stat id.</param>
        /// <returns>True when derived.</returns>
        public bool IsDerived(StatId id) => _formulas.ContainsKey(id);

        /// <summary>
        /// Tries to get the formula of a derived stat.
        /// </summary>
        /// <param name="id">Stat id.</param>
        /// <param name="formula">The formula when derived.</param>
        /// <returns>True when derived.</returns>
        public bool TryGetFormula(StatId id, out IStatFormula formula) => _formulas.TryGetValue(id, out formula);

        private void ValidateParent(StatDefinition definition)
        {
            if (!definition.ParentId.HasValue)
            {
                return;
            }

            StatId parentId = definition.ParentId.Value;
            if (!_definitions.TryGetValue(parentId, out StatDefinition parent) || parent.Layer != StatLayer.Core)
            {
                throw new ArgumentException($"Stat '{definition.Id}' has parent '{parentId}' which is not a core stat.");
            }
        }

        private void AddFormula(IStatFormula formula)
        {
            if (formula == null)
            {
                throw new ArgumentException("Formulas must not contain null.");
            }
            if (!_definitions.TryGetValue(formula.Target, out StatDefinition target) || target.Layer != StatLayer.Core)
            {
                throw new ArgumentException($"Formula target '{formula.Target}' is not a core stat.");
            }
            if (_formulas.ContainsKey(formula.Target))
            {
                throw new ArgumentException($"Stat '{formula.Target}' has more than one formula.");
            }

            _formulas.Add(formula.Target, formula);
            foreach (StatId input in formula.Inputs)
            {
                if (!_definitions.ContainsKey(input))
                {
                    throw new ArgumentException($"Formula '{formula.Target}' reads unknown stat '{input}'.");
                }
            }

            // Inputs must be stored stats, which rules out cycles entirely.
            foreach (IStatFormula existing in _formulas.Values)
            {
                foreach (StatId input in existing.Inputs.Where(_formulas.ContainsKey))
                {
                    throw new ArgumentException($"Formula '{existing.Target}' reads derived stat '{input}'.");
                }
            }
        }
    }
}
