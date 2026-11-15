using System;
using System.Globalization;
using BetterLife.Core.Domain.Characters;

namespace BetterLife.Core.Rules.Serialization
{
    /// <summary>
    /// State while reading rule JSON: the catalogs used to validate referenced ids and the current JSON path.
    /// </summary>
    public sealed class RuleReadContext
    {
        /// <summary>Path of the document root.</summary>
        public const string RootPath = "$";

        /// <summary>
        /// Creates a context.
        /// </summary>
        /// <param name="catalogs">Catalogs used to validate ids; null while reading the catalogs themselves.</param>
        /// <param name="path">Current JSON path.</param>
        public RuleReadContext(CharacterCatalogs catalogs, string path = RootPath)
        {
            Catalogs = catalogs;
            Path = string.IsNullOrEmpty(path) ? RootPath : path;
        }

        /// <summary>Catalogs used to validate ids, or null.</summary>
        public CharacterCatalogs Catalogs { get; }

        /// <summary>
        /// Returns the catalogs, or throws when this context was created without them.
        /// </summary>
        /// <returns>The catalogs.</returns>
        public CharacterCatalogs RequireCatalogs()
        {
            if (Catalogs == null)
            {
                throw new InvalidOperationException("This read context has no catalogs; id references cannot be checked here.");
            }
            return Catalogs;
        }

        /// <summary>Current JSON path.</summary>
        public string Path { get; }

        /// <summary>
        /// Returns a context for a property of the current object.
        /// </summary>
        /// <param name="name">Property name.</param>
        /// <returns>The child context.</returns>
        public RuleReadContext Property(string name) => new RuleReadContext(Catalogs, Path + "." + name);

        /// <summary>
        /// Returns a context for an element of the current array.
        /// </summary>
        /// <param name="index">Element index.</param>
        /// <returns>The child context.</returns>
        public RuleReadContext Index(int index) => new RuleReadContext(Catalogs, Path + "[" + index.ToString(CultureInfo.InvariantCulture) + "]");

        /// <summary>
        /// Creates an exception for the current path.
        /// </summary>
        /// <param name="problem">What is wrong.</param>
        /// <param name="inner">Underlying exception, or null.</param>
        /// <returns>The exception to throw.</returns>
        public RuleParseException Error(string problem, Exception inner = null) => new RuleParseException(Path, problem, inner);
    }
}
