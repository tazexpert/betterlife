using System.Collections.Generic;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// Turns all files of one content type into a product (usually a catalog) added to the repository.
    /// A loader may also have no files and only combine earlier products.
    /// </summary>
    public interface IContentTypeLoader
    {
        /// <summary>Content type name used in the manifest, for example "traits".</summary>
        string TypeName { get; }

        /// <summary>Type names whose products must be loaded first.</summary>
        IReadOnlyList<string> DependsOn { get; }

        /// <summary>True when the manifest may list files of this type.</summary>
        bool AcceptsFiles { get; }

        /// <summary>
        /// Loads the documents. Report problems through the context instead of throwing.
        /// </summary>
        /// <param name="documents">All files of this type, in manifest order.</param>
        /// <param name="context">Access to earlier products, rules and error reporting.</param>
        void Load(IReadOnlyList<ContentDocument> documents, ContentLoadContext context);
    }
}
