namespace BetterLife.Core.Content
{
    /// <summary>
    /// One problem found while loading content, located by file and JSON path.
    /// </summary>
    public sealed class ContentError
    {
        /// <summary>
        /// Creates an error.
        /// </summary>
        /// <param name="file">Content-relative file path, or empty for pipeline-level problems.</param>
        /// <param name="path">JSON path inside the file, or empty.</param>
        /// <param name="message">What is wrong.</param>
        public ContentError(string file, string path, string message)
        {
            File = file ?? string.Empty;
            Path = path ?? string.Empty;
            Message = message ?? string.Empty;
        }

        /// <summary>Content-relative file path.</summary>
        public string File { get; }

        /// <summary>JSON path inside the file.</summary>
        public string Path { get; }

        /// <summary>What is wrong.</summary>
        public string Message { get; }

        /// <summary>Single-line form such as "traits.json $.traits[2].id: duplicate id".</summary>
        /// <returns>The formatted error.</returns>
        public override string ToString()
        {
            string location = string.IsNullOrEmpty(Path) ? File : $"{File} {Path}";
            return string.IsNullOrEmpty(location) ? Message : $"{location}: {Message}";
        }
    }
}
