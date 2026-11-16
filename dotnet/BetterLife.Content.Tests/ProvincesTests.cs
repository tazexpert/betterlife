using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Localization;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Real content: the creation screen offers all 77 Thai provinces in Thai alphabetical order; places kept only for old
    /// saves are hidden. Every life is born in Thailand.
    /// </summary>
    [TestFixture]
    public class ProvincesTests
    {
        [Test]
        public void Thailand_HasAll77Provinces_InThaiOrder()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var th = new TableLocalizer(content.Get<StringTables>(), "th", "en");
            var form = new CreationFormViewModel(content, th);

            var provinces = form.Cities("thailand");
            Assert.That(provinces, Has.Count.EqualTo(77));
            Assert.That(provinces.Select(p => p.Id), Does.Not.Contain("thailand.rural_isan"), "kept only for old saves");
            Assert.That(provinces.First().Label, Is.EqualTo("กระบี่"));
            Assert.That(provinces.Select(p => p.Label).ToList().IndexOf("เชียงใหม่"), Is.LessThan(provinces.Select(p => p.Label).ToList().IndexOf("ตาก")),
                "a leading vowel sorts by its consonant (ช before ต)");
            Assert.That(form.Families, Has.Count.EqualTo(10));
            Assert.That(form.HomeCountryId, Is.EqualTo("thailand"), "the form shows no country choice");
            for (ulong seed = 1; seed <= 40; seed++)
            {
                var c = new BetterLife.Core.Creation.CharacterCreator(content)
                    .Create(new BetterLife.Core.Creation.CreationRequest(seed, BetterLife.Core.Creation.CreationMode.Random)).Character;
                Assert.That(c.Residence.CountryId, Is.EqualTo("thailand"), "every life is born in Thailand");
            }
        }
    }
}
