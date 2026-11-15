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

        [Test]
        public void EveryEventChoice_HasAConsequence()
        {
            // Rule from the user (task 16b): every outcome of every choice changes something, and an event
            // without choices does something when it fires.
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var problems = new System.Collections.Generic.List<string>();
            foreach (BetterLife.Core.LifeEvents.LifeEventDefinition e in content.Get<BetterLife.Core.Domain.DefinitionCatalog<BetterLife.Core.LifeEvents.LifeEventDefinition>>().All)
            {
                if (!e.HasChoices && e.OnFire.Effects.Count == 0)
                {
                    problems.Add(e.Id + ": no choices and no onFire effects");
                }
                foreach (BetterLife.Core.LifeEvents.EventChoice choice in e.Choices)
                {
                    if (choice.Success.Effects.Effects.Count == 0)
                    {
                        problems.Add(e.Id + "/" + choice.Id + ": success has no effects");
                    }
                    if (choice.Failure != null && choice.Failure.Effects.Effects.Count == 0)
                    {
                        problems.Add(e.Id + "/" + choice.Id + ": failure has no effects");
                    }
                }
            }
            Assert.That(problems, Is.Empty);
        }

        [Test]
        public void RealLives_AreNotAskedToDecide_BeforeTheMinimumAge()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            int minAge = content.Get<BetterLife.Core.LifeEvents.EventSettings>().MinDecisionAge;
            Assert.That(minAge, Is.GreaterThan(0));
            var events = content.Get<BetterLife.Core.Domain.DefinitionCatalog<BetterLife.Core.LifeEvents.LifeEventDefinition>>();
            for (int i = 0; i < 20; i++)
            {
                var creation = new BetterLife.Core.Creation.CharacterCreator(content).Create(
                    new BetterLife.Core.Creation.CreationRequest((ulong)(4200 + i * 13), BetterLife.Core.Creation.CreationMode.Random));
                using (var life = BetterLife.Core.Sessions.LifeSessionFactory.CreateNew(content, creation, new BetterLife.Core.Events.EventBus(), RepoContent.Simulation))
                {
                    while (!life.IsOver && life.Character.AgeAt(life.TimeState.Clock.Now) < minAge)
                    {
                        life.Advance(BetterLife.Core.Time.Ticking.TimeSkip.Month);
                    }
                    var decisions = life.Events.State.History.Where(h => life.Character.AgeAt(h.Week) < minAge && events.TryGet(h.EventId, out var e) && e.HasChoices && e.Trigger == BetterLife.Core.LifeEvents.EventTrigger.Random)
                        .Select(h => h.EventId + " at age " + life.Character.AgeAt(h.Week)).ToList();
                    Assert.That(decisions, Is.Empty, $"life {i} was asked to decide before age {minAge}");
                }
            }
        }
    }
}
