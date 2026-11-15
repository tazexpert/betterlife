namespace BetterLife.Core.Content
{
    /// <summary>
    /// One file listed in the manifest.
    /// </summary>
    public sealed class ContentFileEntry
    {
        /// <summary>
        /// Creates an entry.
        /// </summary>
        /// <param name="type">Content type name, for example "traits".</param>
        /// <param name="path">Content-relative path.</param>
        public ContentFileEntry(string type, string path)
        {
            Type = type;
            Path = path;
        }

        /// <summary>Content type name.</summary>
        public string Type { get; }

        /// <summary>Content-relative path.</summary>
        public string Path { get; }
    }
}
