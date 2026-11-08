using System;
using System.IO;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Infrastructure.Content;
using UnityEditor;
using UnityEngine;

namespace BetterLife.EditorTools
{
    /// <summary>
    /// Validates the repository's content and copies it into Resources so builds can read it.
    /// The copy is git-ignored; /content stays the single source of truth.
    /// </summary>
    public static class ContentSync
    {
        private const string MenuPath = "BetterLife/Validate and Copy Content";
        private const string LogPrefix = "[BetterLife] ";

        /// <summary>
        /// Menu entry: validates and copies, logging the outcome.
        /// </summary>
        [MenuItem(MenuPath)]
        public static void ValidateAndCopyFromMenu()
        {
            string error = ValidateAndCopy();
            if (error == null)
            {
                Debug.Log(LogPrefix + "Content is valid and was copied to " + ContentLocations.ResourcesCopyAssetPath + ".");
            }
            else
            {
                Debug.LogError(LogPrefix + error);
            }
        }

        /// <summary>
        /// Loads the repository content through the game's pipeline and, when valid, replaces the Resources copy.
        /// </summary>
        /// <returns>Null on success, otherwise a description of every content error.</returns>
        public static string ValidateAndCopy()
        {
            string source = ContentLocations.RepositoryContentPath;
            if (!Directory.Exists(source))
            {
                return "Content folder not found: " + source;
            }

            ContentLoadResult result = DefaultContent.CreatePipeline(new FileSystemContentSource(source)).Load();
            if (!result.Succeeded)
            {
                return "Content has errors:" + Environment.NewLine + string.Join(Environment.NewLine, result.Errors.Select(e => "  " + e));
            }

            string target = ContentLocations.ResourcesCopyPath;
            if (Directory.Exists(target))
            {
                Directory.Delete(target, true);
            }
            foreach (string file in Directory.GetFiles(source, "*" + ContentPaths.JsonExtension, SearchOption.AllDirectories))
            {
                string destination = Path.Combine(target, Path.GetRelativePath(source, file));
                Directory.CreateDirectory(Path.GetDirectoryName(destination));
                File.Copy(file, destination, true);
            }
            AssetDatabase.Refresh();
            return null;
        }
    }
}
