using System;
using System.Collections.Generic;

namespace BetterLife.Core.Domain.Stats
{
    /// <summary>
    /// Hidden reputation per social domain (for example "business" or "underworld"), 0-100 where 0 means unknown.
    /// Only declared domains can be used.
    /// </summary>
    public sealed class ReputationBook
    {
        /// <summary>Reputation of a domain nobody in it knows the character.</summary>
        public const float Unknown = StatBounds.Min;

        private readonly Dictionary<string, float> _values = new Dictionary<string, float>(StringComparer.Ordinal);
        private readonly DefinitionCatalog<ReputationDomainDefinition> _domains;

        /// <summary>
        /// Creates an empty book.
        /// </summary>
        /// <param name="domains">Declared domains.</param>
        public ReputationBook(DefinitionCatalog<ReputationDomainDefinition> domains)
        {
            _domains = domains ?? throw new ArgumentNullException(nameof(domains));
        }

        /// <summary>Domains with a non-default reputation.</summary>
        public IReadOnlyDictionary<string, float> Values => _values;

        /// <summary>
        /// Returns the reputation in a domain.
        /// </summary>
        /// <param name="domainId">Declared domain id.</param>
        /// <returns>Value from 0 to 100.</returns>
        public float Get(string domainId)
        {
            _domains.Get(domainId);
            return _values.TryGetValue(domainId, out float value) ? value : Unknown;
        }

        /// <summary>
        /// Changes the reputation in a domain, clamped to 0-100.
        /// </summary>
        /// <param name="domainId">Declared domain id.</param>
        /// <param name="delta">Finite change.</param>
        /// <returns>The new value.</returns>
        public float Change(string domainId, float delta)
        {
            if (float.IsNaN(delta) || float.IsInfinity(delta))
            {
                throw new ArgumentException("Delta must be finite.", nameof(delta));
            }
            return Set(domainId, Get(domainId) + delta);
        }

        /// <summary>
        /// Sets the reputation in a domain, clamped to 0-100.
        /// </summary>
        /// <param name="domainId">Declared domain id.</param>
        /// <param name="value">New value.</param>
        /// <returns>The value stored.</returns>
        public float Set(string domainId, float value)
        {
            _domains.Get(domainId);
            if (float.IsNaN(value))
            {
                throw new ArgumentException("Value must not be NaN.", nameof(value));
            }

            float clamped = StatBounds.Clamp(value);
            _values[domainId] = clamped;
            return clamped;
        }
    }
}
