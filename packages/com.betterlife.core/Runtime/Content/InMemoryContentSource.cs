using System;
using System.Collections.Generic;
using System.IO;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// Holds content files in memory, for tests.
    /// </summary>
    public sealed class InMemoryContentSource : IContentSource
    {
        private readonly Dictionary<string, string> _files = new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>
        /// Adds or replaces a file.
        /// </summary>
        /// <param name="relativePath">Content-relative path.</param>
        /// <param name="text">File text.</param>
        /// <returns>This source, for chaining.</returns>
        public InMemoryContentSource Add(string relativePath, string text)
        {
            _files[relativePath] = text ?? throw new ArgumentNullException(nameof(text));
            return this;
        }

        /// <inheritdoc />
        public string ReadText(string relativePath)
        {
            if (relativePath == null || !_files.TryGetValue(relativePath, out string text))
            {
                throw new FileNotFoundException($"Content file '{relativePath}' not found.");
            }
            return text;
        }
    }
}
