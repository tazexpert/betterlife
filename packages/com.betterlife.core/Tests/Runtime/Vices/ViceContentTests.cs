using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Tests.Careers;
using BetterLife.Core.Tests.Content;
using BetterLife.Core.Vices;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Vices
{
    /// <summary>Tests for the vice loaders.</summary>
    [TestFixture]
    public class ViceContentTests
    {
        [Test]
        public void TestContent_LoadsVicesMethodsAndSettings()
        {
            IContentRepository content = SchoolWorkFixture.Content;
            var vices = content.Get<DefinitionCatalog<ViceDefinition>>();

            Assert.That(vices.Get("betting").IsGambling && vices.Get("betting").BlocksUndo, Is.True);
            Assert.That(vices.Get("betting").Gambling.ExpectedValuePerBaht, Is.EqualTo(-0.1m));
            Assert.That(vices.Get("drinking").StageMultiplier("adult"), Is.EqualTo(1.5f));
            Assert.That(vices.Get("drinking").StageMultiplier("child"), Is.EqualTo(1f));
            Assert.That(vices.Get("gaming").Activity.UsesFor(125), Is.EqualTo(2));
            Assert.That(content.Get<DefinitionCatalog<QuitMethodDefinition>>().Get("rehab").AppliesTo("betting"), Is.False);
            Assert.That(content.Get<ViceSettings>().TraitMultipliers["anxious"], Is.EqualTo(1.5f));
        }

        [TestCase("vices.json", "'stage': 'adult'", "'stage': 'ancient'", "unknown life stage 'ancient'")]
        [TestCase("vices.json", "'id': 'free_time', 'freeHours'", "'id': 'arcade', 'freeHours'", "unknown activity 'arcade'")]
        [TestCase("vices.json", "'severity': 'minor'", "'severity': 'apocalyptic'", "'apocalyptic' is not one of")]
        [TestCase("quit_methods.json", "'vices': [ 'drinking' ]", "'vices': [ 'chess' ]", "unknown vice")]
        [TestCase("vice_settings.json", "'traits': [ 'anxious' ]", "'traits': [ 'evil' ]", "unknown trait")]
        [TestCase("vice_settings.json", "'activities': [ 'free_time' ]", "'activities': [ 'tv' ]", "unknown activity")]
        public void InvalidContent_IsReported(string file, string find, string replace, string message)
        {
            string original = file == "vices.json" ? TestContent.Vices : file == "quit_methods.json" ? TestContent.QuitMethods : TestContent.ViceSettings;
            Assert.That(original, Does.Contain(find));
            InMemoryContentSource source = TestContent.Source().Add(file, original.Replace(find, replace));

            ContentLoadResult result = DefaultContent.CreatePipeline(source).Load();

            Assert.That(result.Errors.Select(e => e.Message), Has.Some.Contains(message), string.Join("\n", result.Errors));
        }
    }
}
