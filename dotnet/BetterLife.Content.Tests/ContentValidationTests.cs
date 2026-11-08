using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using BetterLife.Core.Content;
using Json.Schema;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Validates the real content folder: every file matches its schema, the manifest matches the folder,
    /// and the whole content loads through the game's pipeline (which checks every referenced id).
    /// </summary>
    [TestFixture]
    public class ContentValidationTests
    {
        private ContentManifest _manifest;
        private EvaluationOptions _options;

        [OneTimeSetUp]
        public void LoadManifestAndSchemas()
        {
            var errors = new List<ContentError>();
            _manifest = ContentManifest.Read(ContentJson.ParseObject(File.ReadAllText(Path.Combine(RepoContent.ContentDir, ContentPaths.ManifestFile))), errors);
            Assert.That(errors, Is.Empty, string.Join("\n", errors));

            _options = new EvaluationOptions { OutputFormat = OutputFormat.List };
            foreach (string file in Directory.GetFiles(RepoContent.SchemaDir, "*.schema.json"))
            {
                _options.SchemaRegistry.Register(JsonSchema.FromFile(file));
            }
        }

        [Test]
        public void Manifest_MatchesItsSchema()
        {
            AssertValid(ContentPaths.ManifestFile, "manifest");
        }

        [Test]
        public void EveryContentType_HasASchema()
        {
            IEnumerable<string> missing = _manifest.Files.Select(f => f.Type).Distinct().Where(t => !File.Exists(RepoContent.SchemaPath(t)));

            Assert.That(missing, Is.Empty);
        }

        [Test]
        public void EveryListedFile_MatchesItsSchema()
        {
            var failures = new List<string>();
            foreach (ContentFileEntry entry in _manifest.Files)
            {
                failures.AddRange(Validate(entry.Path, entry.Type));
            }

            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }

        [Test]
        public void Manifest_ListsExactlyTheFilesInTheFolder()
        {
            string root = RepoContent.ContentDir;
            var onDisk = Directory.GetFiles(root, "*.json", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(root, f).Replace(Path.DirectorySeparatorChar, '/'))
                .Where(f => f != ContentPaths.ManifestFile)
                .ToList();
            var listed = _manifest.Files.Select(f => f.Path).ToList();

            Assert.That(onDisk.Except(listed), Is.Empty, "files missing from manifest.json");
            Assert.That(listed.Except(onDisk), Is.Empty, "manifest.json lists files that do not exist");
        }

        [Test]
        public void AllContent_LoadsThroughThePipeline()
        {
            ContentLoadResult result = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load();

            Assert.That(result.Errors.Select(e => e.ToString()), Is.Empty);
            Assert.That(result.Repository.ContentVersion, Is.EqualTo(_manifest.ContentVersion));
        }

        [Test]
        public void SchemaValidation_CatchesTypos()
        {
            JsonNode broken = JsonNode.Parse("{ \"skills\": [ { \"id\": \"cooking\", \"nameKey\": \"skill.cooking.name\", \"category\": \"life\", \"gainFalloffExponent\": 1, \"decay\": { \"graceWeeks\": 1, \"ratePerWeek\": 1, \"retentionOfPeak\": 2 }, \"colour\": \"red\" } ] }");

            EvaluationResults results = Schema("skills").Evaluate(broken, _options);

            Assert.That(results.IsValid, Is.False);
        }

        private void AssertValid(string relativePath, string type)
        {
            List<string> failures = Validate(relativePath, type).ToList();
            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }

        private IEnumerable<string> Validate(string relativePath, string type)
        {
            JsonNode instance = JsonNode.Parse(File.ReadAllText(Path.Combine(RepoContent.ContentDir, relativePath)));
            EvaluationResults results = Schema(type).Evaluate(instance, _options);
            if (results.IsValid)
            {
                return Enumerable.Empty<string>();
            }
            return results.Details
                .Where(d => d.Errors != null)
                .SelectMany(d => d.Errors.Select(e => $"{relativePath} {d.InstanceLocation}: {e.Value}"))
                .DefaultIfEmpty($"{relativePath}: does not match {type}.schema.json");
        }

        private static JsonSchema Schema(string type)
        {
            return JsonSchema.FromFile(RepoContent.SchemaPath(type));
        }
    }
}
