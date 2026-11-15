using System;
using Newtonsoft.Json.Linq;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// A parsed content file.
    /// </summary>
    public sealed class ContentDocument
    {
        /// <summary>
        /// Creates a document.
        /// </summary>
        /// <param name="path">Content-relative path.</param>
        /// <param name="root">Parsed root object.</param>
        public ContentDocument(string path, JObject root)
        {
            Path = path ?? throw new ArgumentNullException(nameof(path));
            Root = root ?? throw new ArgumentNullException(nameof(root));
        }

        /// <summary>Content-relative path.</summary>
        public string Path { get; }

        /// <summary>Parsed root object.</summary>
        public JObject Root { get; }
    }
}
