using System;
using System.Linq;

namespace BetterLife.Core.Content
{
    /// <summary>
    /// Validation of content-relative paths: forward slashes, no rooted paths, no "..", ".json" extension.
    /// </summary>
    public static class ContentPaths
    {
        /// <summary>Name of the manifest at the content root.</summary>
        public const string ManifestFile = "manifest.json";

        /// <summary>Required extension of content files.</summary>
        public const string JsonExtension = ".json";

        private const char Separator = '/';
        private const string ParentSegment = "..";

        /// <summary>
        /// Returns true when the path is a safe content-relative JSON path.
        /// </summary>
        /// <param name="path">Candidate path.</param>
        /// <returns>True when valid.</returns>
        public static bool IsValid(string path)
        {
            if (string.IsNullOrEmpty(path) || path.Contains('\\') || path.StartsWith(Separator.ToString(), StringComparison.Ordinal) || path.Contains(':'))
            {
                return false;
            }
            if (!path.EndsWith(JsonExtension, StringComparison.Ordinal))
            {
                return false;
            }
            return path.Split(Separator).All(segment => segment.Length > 0 && segment != ParentSegment && segment != ".");
        }
    }
}
