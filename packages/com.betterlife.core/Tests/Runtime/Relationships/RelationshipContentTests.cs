using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Careers;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Karma;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Relationships;
using BetterLife.Core.Tests.Careers;
using BetterLife.Core.Tests.Content;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Relationships
{
    /// <summary>Tests for the relationship loaders and the career network.</summary>
    [TestFixture]
    public class RelationshipContentTests
    {
        [Test]
        public void TestContent_LoadsSettingsAndInteractions()
        {
            IContentRepository content = SchoolWorkFixture.Content;
            var settings = content.Get<RelationshipSettings>();
            var interactions = content.Get<DefinitionCatalog<InteractionDefinition>>();

            Assert.That(settings.Types, Has.Count.EqualTo(9));
            Assert.That(settings.Family.ParentValue("warm"), Is.EqualTo(70f));
            Assert.That(settings.Family.ParentValue("unknown"), Is.EqualTo(50f));
            Assert.That(interactions.Get("ask_out").Success.Becomes, Is.EqualTo(RelationshipType.Partner));
            Assert.That(interactions.Get("try_for_baby").Success.Child, Is.EqualTo(ChildMode.Birth));
            Assert.That(interactions.Get("chat").Chance, Is.Null);
        }

        [TestCase("relationship_settings.json", "{ 'type': 'contact', 'monthlyDecay': 1, 'floor': 10, 'griefStress': 3, 'griefHappiness': -2 } ],", "],", "Rules for 'contact' are missing")]
        [TestCase("relationship_settings.json", "'personality': 'strict'", "'personality': 'cold'", "unknown parent personality 'cold'")]
        [TestCase("relationship_settings.json", "'socialActivity': 'socialize'", "'socialActivity': 'partying'", "unknown activity 'partying'")]
        [TestCase("interactions.json", "'becomes': 'partner'", "'becomes': 'soulmate'", "'soulmate' is not one of")]
        [TestCase("interactions.json", "'child': 'adoption'", "'child': 'stork'", "'stork' is not one of")]
        [TestCase("interactions.json", "'types': [ 'friend' ]", "'types': [ 'pet' ]", "'pet' is not one of")]
        public void InvalidContent_IsReported(string file, string find, string replace, string message)
        {
            string original = file == "interactions.json" ? TestContent.Interactions : TestContent.RelationshipSettings;
            Assert.That(original, Does.Contain(find));
            InMemoryContentSource source = TestContent.Source().Add(file, original.Replace(find, replace));

            ContentLoadResult result = DefaultContent.CreatePipeline(source).Load();

            Assert.That(result.Errors.Select(e => e.Message), Has.Some.Contains(message), string.Join("\n", result.Errors));
        }

        [Test]
        public void CareerService_AddsTheNetworkBonusToHiringOdds()
        {
            var plain = new SchoolWorkFixture(2030);
            var connected = new SchoolWorkFixture(2030);
            var network = new FixedNetwork();
            var service = new CareerService(connected.Character, connected.Tracks, connected.CareerSettings, connected.Programs, connected.Budget,
                connected.TimeSettings, new LuckCalculator(SchoolWorkFixture.Content.Get<LuckSettings>(),
                    SchoolWorkFixture.Content.Get<FortuneCalculator>(), SchoolWorkFixture.Content.Get<KarmaService>()),
                new Pcg32(1UL, 1UL), connected.Economy, connected.Bus, connected.Clock, network);

            HiringQuote without = plain.Career.Quote("courier");
            HiringQuote with = service.Quote("courier");

            Assert.That(with.NetworkAdjustment, Is.EqualTo(12f));
            Assert.That(without.NetworkAdjustment, Is.EqualTo(0f));
            Assert.That(with.Chance.Final, Is.GreaterThan(without.Chance.Final));
        }

        private sealed class FixedNetwork : ICareerNetwork
        {
            public float HiringBonus(CareerTrackDefinition track) => 12f;
        }
    }
}
