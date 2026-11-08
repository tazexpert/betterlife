using System.IO;
using UnityEngine;

namespace BetterLife.Infrastructure.Content
{
    /// <summary>
    /// Where content lives: the repository's /content folder in the editor, a Resources copy in builds.
    /// </summary>
    public static class ContentLocations
    {
        /// <summary>Folder name of the content source of truth at the repository root.</summary>
        public const string RepositoryContentFolder = "content";

        /// <summary>Resources sub-folder holding the build-time copy (Resources.Load paths start here).</summary>
        public const string ResourcesFolder = "Content";

        /// <summary>Project-relative path of the build-time copy.</summary>
        public const string ResourcesCopyAssetPath = "Assets/BetterLife/Resources/" + ResourcesFolder;

        /// <summary>
        /// Absolute path of the repository's content folder. The Unity project lives at &lt;repo&gt;/unity,
        /// so the repository root is the parent of the project folder.
        /// </summary>
        public static string RepositoryContentPath =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", RepositoryContentFolder));

        /// <summary>Absolute path of the build-time copy.</summary>
        public static string ResourcesCopyPath =>
            Path.GetFullPath(Path.Combine(Application.dataPath, "..", ResourcesCopyAssetPath));
    }
}
