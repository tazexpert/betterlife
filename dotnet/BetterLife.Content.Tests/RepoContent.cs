using System.IO;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Locates the repository's content and schema folders.
    /// </summary>
    internal static class RepoContent
    {
        private const int MaxParentLookups = 10;

        public static string Root
        {
            get
            {
                var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
                for (int i = 0; i < MaxParentLookups && dir != null; i++, dir = dir.Parent)
                {
                    if (File.Exists(Path.Combine(dir.FullName, "content", "manifest.json")))
                    {
                        return dir.FullName;
                    }
                }
                Assert.Fail("Repository root with content/manifest.json not found.");
                return null;
            }
        }

        public static string ContentDir => Path.Combine(Root, "content");

        public static string SchemaDir => Path.Combine(Root, "docs", "schemas");

        public static string SchemaPath(string type) => Path.Combine(SchemaDir, type + ".schema.json");
    }
}
