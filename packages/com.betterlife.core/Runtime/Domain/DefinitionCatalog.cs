using System;
using System.Collections.Generic;

namespace BetterLife.Core.Domain
{
    /// <summary>
    /// Immutable lookup of content definitions by id. Rejects duplicates at construction.
    /// </summary>
    /// <typeparam name="T">Definition type.</typeparam>
    public sealed class DefinitionCatalog<T> where T : class, IDefinition
    {
        private readonly Dictionary<string, T> _byId = new Dictionary<string, T>(StringComparer.Ordinal);
        private readonly List<T> _ordered = new List<T>();

        /// <summary>
        /// Creates a catalog from definitions.
        /// </summary>
        /// <param name="definitions">Definitions with unique ids.</param>
        public DefinitionCatalog(IEnumerable<T> definitions)
        {
            if (definitions == null)
            {
                throw new ArgumentNullException(nameof(definitions));
            }

            foreach (T definition in definitions)
            {
                if (definition == null)
                {
                    throw new ArgumentException("Definitions must not contain null.", nameof(definitions));
                }
                if (_byId.ContainsKey(definition.Id))
                {
                    throw new ArgumentException($"Duplicate definition id '{definition.Id}'.", nameof(definitions));
                }
                _byId.Add(definition.Id, definition);
                _ordered.Add(definition);
            }
        }

        /// <summary>All definitions in declaration order.</summary>
        public IReadOnlyList<T> All => _ordered;

        /// <summary>
        /// Returns true when a definition with this id exists.
        /// </summary>
        /// <param name="id">Content id.</param>
        /// <returns>True when present.</returns>
        public bool Contains(string id)
        {
            return id != null && _byId.ContainsKey(id);
        }

        /// <summary>
        /// Tries to find a definition.
        /// </summary>
        /// <param name="id">Content id.</param>
        /// <param name="definition">The definition when found.</param>
        /// <returns>True when found.</returns>
        public bool TryGet(string id, out T definition)
        {
            definition = null;
            return id != null && _byId.TryGetValue(id, out definition);
        }

        /// <summary>
        /// Returns a definition or throws when it is unknown.
        /// </summary>
        /// <param name="id">Content id.</param>
        /// <returns>The definition.</returns>
        public T Get(string id)
        {
            if (!TryGet(id, out T definition))
            {
                throw new KeyNotFoundException($"Unknown {typeof(T).Name} id '{id}'.");
            }
            return definition;
        }
    }
}
