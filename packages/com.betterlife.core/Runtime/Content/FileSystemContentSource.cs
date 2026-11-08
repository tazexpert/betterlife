using System;
using System.IO;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// Reads content from a folder on disk (editor play mode and tests).
    /// </summary>
    public sealed class FileSystemContentSource : IContentSource
    {
        private readonly string _root;

        /// <summary>
        /// Creates a source.
        /// </summary>
        /// <param name="rootDirectory">Absolute path of the content folder.</param>
        public FileSystemContentSource(string rootDirectory)
        {
            if (string.IsNullOrEmpty(rootDirectory))
            {
                throw new ArgumentException("Root directory is required.", nameof(rootDirectory));
            }
            _root = Path.GetFullPath(rootDirectory);
        }

        /// <summary>Absolute path of the content folder.</summary>
        public string RootDirectory => _root;

        /// <inheritdoc />
        public string ReadText(string relativePath)
        {
            if (!ContentPaths.IsValid(relativePath))
            {
                throw new ArgumentException($"'{relativePath}' is not a valid content path.", nameof(relativePath));
            }
            string fullPath = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(fullPath))
            {
                throw new FileNotFoundException($"Content file '{relativePath}' not found.", fullPath);
            }
            return File.ReadAllText(fullPath);
        }
    }
}
