using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace BetterLife.Core.Tests
{
    /// <summary>
    /// Enforces the layer dependency direction by reading every asmdef in the repository:
    /// Core depends on nothing, Presentation and Infrastructure depend on Core only, and only Bootstrap composes them.
    /// </summary>
    [TestFixture]
    public class AssemblyDependencyRulesTests
    {
        private const string CoreAssembly = "BetterLife.Core";
        private const string InfrastructureAssembly = "BetterLife.Infrastructure";
        private const string PresentationAssembly = "BetterLife.Presentation";
        private const string BootstrapAssembly = "BetterLife.Bootstrap";

        private static readonly string[] AsmdefSearchRoots = { "packages", "unity/Assets" };

        private static readonly Regex NameRegex = new Regex("\"name\"\\s*:\\s*\"([^\"]+)\"");
        private static readonly Regex ReferencesRegex = new Regex("\"references\"\\s*:\\s*\\[(.*?)\\]", RegexOptions.Singleline);
        private static readonly Regex QuotedRegex = new Regex("\"([^\"]+)\"");
        private static readonly Regex NoEngineRegex = new Regex("\"noEngineReferences\"\\s*:\\s*true");

        private Dictionary<string, string> _asmdefTexts;

        [OneTimeSetUp]
        public void LoadAsmdefs()
        {
            string root = RepositoryPaths.RequireRoot();
            _asmdefTexts = AsmdefSearchRoots
                .Select(relative => Path.Combine(root, relative))
                .Where(Directory.Exists)
                .SelectMany(dir => Directory.GetFiles(dir, "*.asmdef", SearchOption.AllDirectories))
                .Select(File.ReadAllText)
                .ToDictionary(text => NameRegex.Match(text).Groups[1].Value, text => text);
        }

        [Test]
        public void Core_HasNoReferences_AndNoEngineReferences()
        {
            string text = RequireAsmdef(CoreAssembly);

            Assert.That(ReferencesOf(text), Is.Empty);
            Assert.That(NoEngineRegex.IsMatch(text), Is.True, "Core asmdef must set noEngineReferences: true.");
        }

        [TestCase(InfrastructureAssembly)]
        [TestCase(PresentationAssembly)]
        public void OuterLayers_ReferenceCoreOnly(string assembly)
        {
            if (!_asmdefTexts.ContainsKey(assembly))
            {
                Assert.Ignore($"{assembly} asmdef not present.");
            }

            Assert.That(ReferencesOf(_asmdefTexts[assembly]), Is.EquivalentTo(new[] { CoreAssembly }));
        }

        [Test]
        public void NothingReferencesBootstrap()
        {
            var offenders = _asmdefTexts
                .Where(pair => ReferencesOf(pair.Value).Contains(BootstrapAssembly))
                .Select(pair => pair.Key)
                .ToList();

            Assert.That(offenders, Is.Empty);
        }

        private string RequireAsmdef(string assembly)
        {
            Assert.That(_asmdefTexts.ContainsKey(assembly), Is.True, $"{assembly} asmdef not found.");
            return _asmdefTexts[assembly];
        }

        private static List<string> ReferencesOf(string asmdefText)
        {
            Match match = ReferencesRegex.Match(asmdefText);
            if (!match.Success)
            {
                return new List<string>();
            }
            return QuotedRegex.Matches(match.Groups[1].Value).Cast<Match>().Select(m => m.Groups[1].Value).ToList();
        }
    }
}
