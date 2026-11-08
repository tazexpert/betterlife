using BetterLife.Core.Content;

namespace BetterLife.Infrastructure.Content
{
    /// <summary>
    /// Picks the content source for the current platform.
    /// </summary>
    public static class ContentSourceFactory
    {
        /// <summary>
        /// Editor: read the repository's /content directly so edits apply on the next Play.
        /// Builds: read the copy in Resources.
        /// </summary>
        /// <returns>The content source.</returns>
        public static IContentSource CreateForCurrentPlatform()
        {
#if UNITY_EDITOR
            return new FileSystemContentSource(ContentLocations.RepositoryContentPath);
#else
            return new ResourcesContentSource();
#endif
        }
    }
}
