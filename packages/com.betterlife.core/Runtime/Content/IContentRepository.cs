namespace BetterLife.Core.Content
{
    /// <summary>
    /// Validated, read-only game content, looked up by product type (for example a trait catalog).
    /// </summary>
    public interface IContentRepository
    {
        /// <summary>Version from the manifest.</summary>
        int ContentVersion { get; }

        /// <summary>
        /// Returns a product or throws when none of that type was loaded.
        /// </summary>
        /// <typeparam name="T">Product type.</typeparam>
        /// <returns>The product.</returns>
        T Get<T>() where T : class;

        /// <summary>
        /// Tries to get a product.
        /// </summary>
        /// <typeparam name="T">Product type.</typeparam>
        /// <param name="product">The product when found.</param>
        /// <returns>True when found.</returns>
        bool TryGet<T>(out T product) where T : class;
    }
}
