using System;
using System.Collections.Generic;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Rules.Serialization;
using Newtonsoft.Json;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// What a loader can use: earlier products, the rule registry, and error reporting.
    /// </summary>
    public sealed class ContentLoadContext
    {
        private readonly Dictionary<Type, object> _products;
        private readonly List<ContentError> _errors;

        internal ContentLoadContext(Dictionary<Type, object> products, List<ContentError> errors, RuleRegistry rules)
        {
            _products = products;
            _errors = errors;
            Rules = rules;
        }

        /// <summary>Registry used to parse conditions and effects.</summary>
        public RuleRegistry Rules { get; }

        /// <summary>Number of errors reported so far by all loaders.</summary>
        public int ErrorCount => _errors.Count;

        /// <summary>
        /// Returns an earlier product; loaders must list its type in <see cref="IContentTypeLoader.DependsOn"/>.
        /// </summary>
        /// <typeparam name="T">Product type.</typeparam>
        /// <returns>The product.</returns>
        public T Require<T>() where T : class
        {
            if (!_products.TryGetValue(typeof(T), out object product))
            {
                throw new InvalidOperationException($"Content product {typeof(T).Name} is not loaded; check the loader's DependsOn.");
            }
            return (T)product;
        }

        /// <summary>
        /// Publishes a product. Each product type may be added once.
        /// </summary>
        /// <typeparam name="T">Product type.</typeparam>
        /// <param name="product">The product.</param>
        public void Add<T>(T product) where T : class
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }
            if (_products.ContainsKey(typeof(T)))
            {
                throw new InvalidOperationException($"Content product {typeof(T).Name} was already added.");
            }
            _products.Add(typeof(T), product);
        }

        /// <summary>
        /// Creates a rule read context rooted at a path, with character catalogs when they are loaded.
        /// </summary>
        /// <param name="path">JSON path.</param>
        /// <returns>The read context.</returns>
        public RuleReadContext ReadContext(string path = RuleReadContext.RootPath)
        {
            _products.TryGetValue(typeof(CharacterCatalogs), out object catalogs);
            return new RuleReadContext(catalogs as CharacterCatalogs, path);
        }

        /// <summary>
        /// Reports a problem.
        /// </summary>
        /// <param name="document">File with the problem.</param>
        /// <param name="path">JSON path.</param>
        /// <param name="message">What is wrong.</param>
        public void ReportError(ContentDocument document, string path, string message)
        {
            _errors.Add(new ContentError(document?.Path, path, message));
        }

        /// <summary>
        /// Runs a read step and converts any content problem into a reported error.
        /// </summary>
        /// <param name="document">File being read.</param>
        /// <param name="path">JSON path used when the problem carries none.</param>
        /// <param name="step">The read step.</param>
        /// <returns>True when the step succeeded.</returns>
        public bool Guard(ContentDocument document, string path, Action step)
        {
            if (step == null)
            {
                throw new ArgumentNullException(nameof(step));
            }
            try
            {
                step();
                return true;
            }
            catch (RuleParseException ex)
            {
                ReportError(document, ex.Path, ex.Problem);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is KeyNotFoundException || ex is FormatException || ex is JsonException)
            {
                ReportError(document, path, ex.Message);
            }
            return false;
        }
    }
}
