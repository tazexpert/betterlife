using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.Traits;
using BetterLife.Core.Rules.Serialization;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Content
{
    /// <summary>Tests for <see cref="ContentPipeline"/> and the built-in loaders, using in-memory files.</summary>
    [TestFixture]
    public class ContentPipelineTests
    {
        private const string Stats = @"{ 'stats': [
            { 'id': 'health', 'layer': 'core', 'polarity': 'higherIsBetter', 'default': 50, 'nameKey': 'stat.health.name', 'descKey': 'stat.health.desc' },
            { 'id': 'health.fitness', 'layer': 'sub', 'parent': 'health', 'polarity': 'higherIsBetter', 'default': 40,
              'nameKey': 'stat.health.fitness.name', 'descKey': 'stat.health.fitness.desc', 'decay': { 'target': 30, 'ratePerWeek': 0.5 } },
            { 'id': 'mental.stress', 'layer': 'hidden', 'polarity': 'lowerIsBetter', 'default': 20, 'nameKey': 'stat.stress.name', 'descKey': 'stat.stress.desc' } ],
          'formulas': [ { 'target': 'health', 'type': 'weighted', 'terms': [ { 'stat': 'health.fitness', 'weight': 1 } ] } ] }";

        private const string Traits = @"{ 'traits': [ { 'id': 'anxious', 'nameKey': 'trait.anxious.name', 'descKey': 'trait.anxious.desc',
            'category': 'personality', 'exclusiveGroup': 'temperament', 'startsHidden': true,
            'statModifiers': [ { 'stat': 'mental.stress', 'direction': 'gain', 'multiplier': 1.25 } ] } ] }";

        private const string Skills = @"{ 'skills': [ { 'id': 'cooking', 'nameKey': 'skill.cooking.name', 'category': 'life',
            'gainFalloffExponent': 1.2, 'decay': { 'graceWeeks': 12, 'ratePerWeek': 0.2, 'retentionOfPeak': 0.6 } } ] }";

        private const string Flags = @"{ 'flags': [
            { 'id': 'career.fired_once', 'kind': 'bool', 'defaultExpiryWeeks': null, 'inheritable': false, 'eulogyKey': 'eulogy.fired' },
            { 'id': 'vice.wins', 'kind': 'counter', 'defaultExpiryWeeks': 52, 'inheritable': true, 'eulogyKey': null } ] }";

        private const string Domains = "{ 'domains': [ { 'id': 'business', 'nameKey': 'reputation.business.name' } ] }";

        private const string Conditions = "{ 'conditions': [ { 'id': 'flu', 'nameKey': 'condition.flu.name', 'descKey': 'condition.flu.desc' } ] }";

        [Test]
        public void ValidContent_LoadsEveryProduct()
        {
            ContentLoadResult result = Load(Valid());

            Assert.That(result.Errors, Is.Empty, string.Join("\n", result.Errors));
            IContentRepository repository = result.RequireRepository();
            Assert.That(repository.ContentVersion, Is.EqualTo(3));
            StatCatalog stats = repository.Get<StatCatalog>();
            Assert.That(stats.IsDerived(new StatId("health")), Is.True);
            Assert.That(stats.GetDefinition(new StatId("health.fitness")).Decay.RatePerWeek, Is.EqualTo(0.5f));
            TraitDefinition anxious = repository.Get<DefinitionCatalog<TraitDefinition>>().Get("anxious");
            Assert.That(anxious.StatModifiers.Single().Multiplier, Is.EqualTo(1.25f));
            FlagDefinition wins = repository.Get<DefinitionCatalog<FlagDefinition>>().Get("vice.wins");
            Assert.That(wins.DefaultExpiryWeeks, Is.EqualTo(52));
            Assert.That(wins.EulogyKey, Is.Null);
            CharacterCatalogs catalogs = repository.Get<CharacterCatalogs>();
            Assert.That(catalogs.HealthConditions.Contains("flu"), Is.True);
            Assert.That(catalogs.ReputationDomains.Contains("business"), Is.True);
        }

        [Test]
        public void SameType_CanSpanFiles_ButIdsMustBeUnique()
        {
            InMemoryContentSource source = Valid(extraFiles: "{ 'type': 'skills', 'path': 'more/skills_2.json' }")
                .Add("more/skills_2.json", Skills.Replace("cooking", "baking"));
            Assert.That(Load(source).RequireRepository().Get<DefinitionCatalog<BetterLife.Core.Domain.Skills.SkillDefinition>>().All, Has.Count.EqualTo(2));

            InMemoryContentSource duplicate = Valid(extraFiles: "{ 'type': 'skills', 'path': 'more/skills_2.json' }")
                .Add("more/skills_2.json", Skills);
            ContentError error = Load(duplicate).Errors.First();
            Assert.That(error.File, Is.EqualTo("more/skills_2.json"));
            Assert.That(error.Path, Is.EqualTo("$.skills[0].id"));
            Assert.That(error.Message, Does.Contain("duplicate id 'cooking' (first defined in skills.json)"));
        }

        [Test]
        public void Errors_AreCollected_WithFileAndPath_AndDependentsAreSkipped()
        {
            InMemoryContentSource source = Valid()
                .Add("traits.json", Traits.Replace("'mental.stress'", "'mental.stres'"))
                .Add("skills.json", Skills.Replace("'graceWeeks': 12", "'graceWeeks': 'soon'"));

            ContentLoadResult result = Load(source);

            Assert.That(result.Succeeded, Is.False);
            Assert.That(result.Repository, Is.Null);
            Assert.That(result.Errors.Select(e => e.ToString()), Is.EqualTo(new[]
            {
                "traits.json $.traits[0].statModifiers[0].stat: unknown stat 'mental.stres'.",
                "skills.json $.skills[0].decay.graceWeeks: expected an integer.",
                "Skipped 'character_catalogs' because 'traits' has errors.",
            }));
            var ex = Assert.Throws<ContentLoadException>(() => result.RequireRepository());
            Assert.That(ex.Errors, Has.Count.EqualTo(3));
        }

        [Test]
        public void StatCatalogProblems_AreReported()
        {
            InMemoryContentSource source = Valid().Add("stats.json", Stats.Replace("'weight': 1", "'weight': 0.5"));

            ContentError error = Load(source).Errors.First();

            Assert.That(error.File, Is.EqualTo("stats.json"));
            Assert.That(error.Message, Does.Contain("Weights must sum to 1"));
        }

        [TestCase("{ 'type': 'stats', 'path': 'stats.json' }, { 'type': 'quests', 'path': 'q.json' }", "$.files[1].type", "Unknown content type 'quests'")]
        [TestCase("{ 'type': 'character_catalogs', 'path': 'c.json' }", "$.files[0].type", "does not take files")]
        [TestCase("{ 'type': 'stats', 'path': '../secret.json' }", "$.files[0].path", "not a valid content file path")]
        [TestCase("{ 'type': 'stats', 'path': 'stats.json' }, { 'type': 'stats', 'path': 'stats.json' }", "$.files[1].path", "listed more than once")]
        [TestCase("{ 'type': 'stats', 'path': 'stats.json', 'note': 'x' }", "$.files[0].note", "unknown property")]
        public void InvalidManifest_IsRejected(string files, string path, string message)
        {
            var source = new InMemoryContentSource().Add(ContentPaths.ManifestFile, "{ 'contentVersion': 1, 'files': [ " + files + " ] }");

            ContentError error = Load(source).Errors.First();

            Assert.That(error.File, Is.EqualTo(ContentPaths.ManifestFile));
            Assert.That(error.Path, Is.EqualTo(path));
            Assert.That(error.Message, Does.Contain(message));
        }

        [Test]
        public void MissingOrBrokenFiles_AreReported()
        {
            Assert.That(Load(new InMemoryContentSource()).Errors.Single().Message, Is.EqualTo("Manifest not found."));
            Assert.That(Load(new InMemoryContentSource().Add(ContentPaths.ManifestFile, "{ nope")).Errors.Single().Message, Does.StartWith("Invalid JSON"));
            Assert.That(Load(new InMemoryContentSource().Add(ContentPaths.ManifestFile, "{ 'contentVersion': 0, 'files': [] }")).Errors.Single().Message, Does.Contain("at least 1"));

            InMemoryContentSource missing = Valid(extraFiles: "{ 'type': 'skills', 'path': 'gone.json' }");
            Assert.That(Load(missing).Errors.Select(e => e.ToString()), Has.Some.EqualTo("gone.json: File listed in the manifest was not found."));
        }

        [Test]
        public void DuplicateJsonProperties_AreRejected()
        {
            InMemoryContentSource source = Valid().Add("skills.json", "{ 'skills': [], 'skills': [] }");

            ContentError error = Load(source).Errors.First();

            Assert.That(error.File, Is.EqualTo("skills.json"));
            Assert.That(error.Message, Does.StartWith("Invalid JSON"));
        }

        [Test]
        public void NullableFields_MustBePresent()
        {
            InMemoryContentSource source = Valid().Add("flags.json", "{ 'flags': [ { 'id': 'a.b', 'kind': 'bool', 'inheritable': false, 'eulogyKey': null } ] }");

            ContentError error = Load(source).Errors.First();

            Assert.That(error.Path, Is.EqualTo("$.flags[0].defaultExpiryWeeks"));
            Assert.That(error.Message, Does.Contain("use null for none"));
        }

        [Test]
        public void UnknownFormulaType_IsReported()
        {
            InMemoryContentSource source = Valid().Add("stats.json", Stats.Replace("'type': 'weighted'", "'type': 'magic'"));

            ContentError error = Load(source).Errors.First();

            Assert.That(error.Path, Is.EqualTo("$.formulas[0].type"));
            Assert.That(error.Message, Does.Contain("unknown formula type 'magic'"));
        }

        [Test]
        public void Pipeline_RejectsBadLoaderSetups()
        {
            var rules = DefaultRules.CreateRegistry();
            var source = new InMemoryContentSource();
            List<IContentTypeLoader> loaders = DefaultContent.CreateLoaders();

            Assert.Throws<ArgumentException>(() => new ContentPipeline(source, rules, loaders.Concat(new[] { loaders[0] })), "duplicate type");
            Assert.Throws<ArgumentException>(() => new ContentPipeline(source, rules, loaders.Skip(1)), "unknown dependency");
            Assert.Throws<ArgumentException>(() => new ContentPipeline(source, rules, new[] { new CycleLoader("a", "b"), new CycleLoader("b", "a") }), "cycle");
        }

        [Test]
        public void Loaders_RunInDependencyOrder()
        {
            List<string> order = DefaultContent.CreatePipeline(new InMemoryContentSource()).Loaders.Select(l => l.TypeName).ToList();

            Assert.That(order.IndexOf(ContentTypes.Stats), Is.LessThan(order.IndexOf(ContentTypes.Traits)));
            Assert.That(order.Last(), Is.EqualTo(ContentTypes.CharacterCatalogs));
        }

        [Test]
        public void Repository_ThrowsForMissingProducts()
        {
            IContentRepository repository = Load(Valid()).RequireRepository();

            Assert.That(repository.TryGet(out ContentPipelineTests _), Is.False);
            Assert.Throws<KeyNotFoundException>(() => repository.Get<ContentPipelineTests>());
        }

        [Test]
        public void ContentPaths_Validation()
        {
            Assert.That(ContentPaths.IsValid("events/career_001.json"), Is.True);
            Assert.That(ContentPaths.IsValid("/abs.json"), Is.False);
            Assert.That(ContentPaths.IsValid("a\\b.json"), Is.False);
            Assert.That(ContentPaths.IsValid("a/../b.json"), Is.False);
            Assert.That(ContentPaths.IsValid("a//b.json"), Is.False);
            Assert.That(ContentPaths.IsValid("notes.txt"), Is.False);
            Assert.That(ContentPaths.IsValid("C:/x.json"), Is.False);
        }

        private static ContentLoadResult Load(IContentSource source)
        {
            return DefaultContent.CreatePipeline(source).Load();
        }

        private static InMemoryContentSource Valid(string extraFiles = null)
        {
            string files = "{ 'type': 'stats', 'path': 'stats.json' }, { 'type': 'traits', 'path': 'traits.json' }, "
                + "{ 'type': 'skills', 'path': 'skills.json' }, { 'type': 'flags', 'path': 'flags.json' }, "
                + "{ 'type': 'reputation_domains', 'path': 'reputation_domains.json' }, { 'type': 'health_conditions', 'path': 'health_conditions.json' }"
                + (extraFiles == null ? string.Empty : ", " + extraFiles);
            return new InMemoryContentSource()
                .Add(ContentPaths.ManifestFile, "{ '$schema': '../docs/schemas/manifest.schema.json', 'contentVersion': 3, 'files': [ " + files + " ] }")
                .Add("stats.json", Stats)
                .Add("traits.json", Traits)
                .Add("skills.json", Skills)
                .Add("flags.json", Flags)
                .Add("reputation_domains.json", Domains)
                .Add("health_conditions.json", Conditions);
        }

        private sealed class CycleLoader : IContentTypeLoader
        {
            public CycleLoader(string name, string dependency)
            {
                TypeName = name;
                DependsOn = new[] { dependency };
            }

            public string TypeName { get; }

            public IReadOnlyList<string> DependsOn { get; }

            public bool AcceptsFiles => true;

            public void Load(IReadOnlyList<ContentDocument> documents, ContentLoadContext context)
            {
            }
        }
    }
}
