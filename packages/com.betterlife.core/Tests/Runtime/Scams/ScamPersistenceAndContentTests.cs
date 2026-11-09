using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Persistence;
using BetterLife.Core.Scams;
using BetterLife.Core.Tests.Careers;
using BetterLife.Core.Tests.Content;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Scams
{
    /// <summary>Tests for the scam save mapper and loaders.</summary>
    [TestFixture]
    public class ScamPersistenceAndContentTests
    {
        [Test]
        public void History_RoundTripsThroughTheCharacterSave()
        {
            Character original = TestCatalogs.NewCharacter();
            GameDate week = GameDate.FromYearWeek(2031, 4);
            original.Scams.Restore(new[] { new ScamIncident(7, "phone_gang", week, 12345.67m, 6000m, true) }, 9, 4, 3, 30000.5m, 8);

            Character copy = CharacterDtoMapper.FromDto(CharacterDtoMapper.ToDto(original), TestCatalogs.All()).Character;

            ScamIncident i = copy.Scams.Incidents.Single();
            Assert.That((i.Id, i.ScamId, i.Week, i.Lost, i.Recovered, i.Reported), Is.EqualTo((7, "phone_gang", week, 12345.67m, 6000m, true)));
            Assert.That((copy.Scams.Targeted, copy.Scams.Detected, copy.Scams.Victimized, copy.Scams.NetLost, copy.Scams.NextIncidentId),
                Is.EqualTo((9, 4, 3, 30000.5m, 8)));
        }

        [Test]
        public void TestContent_LoadsScams()
        {
            var scams = SchoolWorkFixture.Content.Get<DefinitionCatalog<ScamDefinition>>();

            Assert.That(scams.Get("phone_gang").Channel, Is.EqualTo(ScamChannel.Phone));
            Assert.That(scams.Get("phone_gang").AgeMultiplier(59), Is.EqualTo(1f));
            Assert.That(scams.Get("phone_gang").AgeMultiplier(61), Is.EqualTo(2.5f));
            Assert.That(SchoolWorkFixture.Content.Get<ScamSettings>().MajorLoss, Is.EqualTo(20000m));
        }

        [TestCase("scams.json", "'channel': 'phone'", "'channel': 'carrier_pigeon'", "'carrier_pigeon' is not one of")]
        [TestCase("scams.json", "'cashShare': 0.5, 'savingsShare': 0.5", "'cashShare': 1.5, 'savingsShare': 0.5", "out of range")]
        [TestCase("scams.json", "'flag': 'career.fired_once'", "'flag': 'career.nope'", "career.nope")]
        [TestCase("scam_settings.json", "'onlineActivities': [ 'socialize' ]", "'onlineActivities': [ 'doomscrolling' ]", "unknown activity")]
        public void InvalidContent_IsReported(string file, string find, string replace, string message)
        {
            string original = file == "scams.json" ? TestContent.Scams : TestContent.ScamSettings;
            Assert.That(original, Does.Contain(find));
            InMemoryContentSource source = TestContent.Source().Add(file, original.Replace(find, replace));

            ContentLoadResult result = DefaultContent.CreatePipeline(source).Load();

            Assert.That(result.Errors.Select(e => e.Message), Has.Some.Contains(message), string.Join("\n", result.Errors));
        }
    }
}
