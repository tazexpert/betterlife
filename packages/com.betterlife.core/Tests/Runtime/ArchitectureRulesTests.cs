using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace BetterLife.Core.Tests
{
    /// <summary>
    /// Static checks that keep the Core runtime sources compliant with the project's architecture rules.
    /// </summary>
    [TestFixture]
    public class ArchitectureRulesTests
    {
        private const string RuntimeRelativePath = "packages/com.betterlife.core/Runtime";
        private const int MaxParentLookups = 10;

        private static readonly Dictionary<string, Regex> ForbiddenPatterns = new Dictionary<string, Regex>
        {
            { "UnityEngine reference", new Regex(@"\bUnityEngine\b") },
            { "System.Random usage", new Regex(@"\bSystem\.Random\b|\bnew\s+Random\s*\(") },
            { "record type", new Regex(@"^\s*(public|internal|private|protected|sealed|abstract|\s)*\s*record\s+\w", RegexOptions.Multiline) },
            { "init-only setter", new Regex(@"\binit\s*;") },
            { "file-scoped namespace", new Regex(@"^\s*namespace\s+[\w\.]+\s*;", RegexOptions.Multiline) },
        };

        /// <summary>
        /// Every Core runtime source file must avoid all forbidden language features and APIs.
        /// </summary>
        [Test]
        public void RuntimeSources_DoNotUseForbiddenApisOrFeatures()
        {
            string runtimeDir = FindRuntimeDirectory();
            if (runtimeDir == null)
            {
                Assert.Ignore("Core runtime source folder not found from the test directory.");
            }

            var violations = new List<string>();
            foreach (string file in Directory.GetFiles(runtimeDir, "*.cs", SearchOption.AllDirectories))
            {
                string text = StripComments(File.ReadAllText(file));
                violations.AddRange(ForbiddenPatterns
                    .Where(rule => rule.Value.IsMatch(text))
                    .Select(rule => $"{Path.GetFileName(file)}: {rule.Key}"));
            }

            Assert.That(violations, Is.Empty, string.Join("\n", violations));
        }

        private static string FindRuntimeDirectory()
        {
            var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            for (int i = 0; i < MaxParentLookups && dir != null; i++, dir = dir.Parent)
            {
                string candidate = Path.Combine(dir.FullName, RuntimeRelativePath);
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
            }
            return null;
        }

        private static string StripComments(string source)
        {
            string noBlock = Regex.Replace(source, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            return Regex.Replace(noBlock, @"//.*$", string.Empty, RegexOptions.Multiline);
        }
    }
}
