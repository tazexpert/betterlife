using System.IO;
using NUnit.Framework;

namespace BetterLife.Core.Tests
{
    /// <summary>
    /// Locates the repository root from the test output folder, for tests that inspect source files.
    /// </summary>
    internal static class RepositoryPaths
    {
        private const string CorePackageRelativePath = "packages/com.betterlife.core";
        private const int MaxParentLookups = 10;

        /// <summary>
        /// Returns the repository root, or null when the tests run from a location without the sources
        /// (for example inside a Unity player build).
        /// </summary>
        /// <returns>Absolute root path or null.</returns>
        public static string FindRoot()
        {
            var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            for (int i = 0; i < MaxParentLookups && dir != null; i++, dir = dir.Parent)
            {
                if (Directory.Exists(Path.Combine(dir.FullName, CorePackageRelativePath)))
                {
                    return dir.FullName;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the repository root or marks the calling test as ignored.
        /// </summary>
        /// <returns>Absolute root path.</returns>
        public static string RequireRoot()
        {
            string root = FindRoot();
            if (root == null)
            {
                Assert.Ignore("Repository sources not found from the test directory.");
            }
            return root;
        }
    }
}
