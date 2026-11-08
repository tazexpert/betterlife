using System;
using System.Collections.Generic;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// Default <see cref="IContentRepository"/>: an immutable map from product type to product.
    /// </summary>
    public sealed class ContentRepository : IContentRepository
    {
        private readonly Dictionary<Type, object> _products;

        /// <summary>
        /// Creates a repository.
        /// </summary>
        /// <param name="contentVersion">Version from the manifest.</param>
        /// <param name="products">Products keyed by their type.</param>
        public ContentRepository(int contentVersion, IReadOnlyDictionary<Type, object> products)
        {
            if (products == null)
            {
                throw new ArgumentNullException(nameof(products));
            }
            ContentVersion = contentVersion;
            _products = new Dictionary<Type, object>();
            foreach (KeyValuePair<Type, object> pair in products)
            {
                _products.Add(pair.Key, pair.Value);
            }
        }

        /// <inheritdoc />
        public int ContentVersion { get; }

        /// <inheritdoc />
        public T Get<T>() where T : class
        {
            if (!TryGet(out T product))
            {
                throw new KeyNotFoundException($"No content product of type {typeof(T).Name} was loaded.");
            }
            return product;
        }

        /// <inheritdoc />
        public bool TryGet<T>(out T product) where T : class
        {
            product = _products.TryGetValue(typeof(T), out object value) ? (T)value : null;
            return product != null;
        }
    }
}
