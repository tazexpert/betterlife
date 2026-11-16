using System.Linq;
using BetterLife.Core.Avatars;
using BetterLife.Core.Content;
using BetterLife.Core.Tests.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Localization;
using BetterLife.Core.Tests.Dashboard;
using BetterLife.Core.Tests.Saving;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Creation
{
    /// <summary>Tests for task 25: the creation screen's draft, options and checks.</summary>
    [TestFixture]
    public class CreationFormTests
    {
        [Test]
        public void ARandomDraft_IsEditable_AndCreatesExactlyWhatWasChosen()
        {
            CreationDraft draft = CreationDraft.Random(SaveFixture.Content, 42UL);
            Assert.That(draft.HasValidDate, Is.True);
            Assert.That(draft.AvatarFace, Is.Not.Null);

            draft.GivenName = "Malee";
            draft.Sex = BiologicalSex.Female;
            draft.Year = 1990;
            draft.Month = 2;
            draft.Day = 28;
            draft.Hour = 6;
            draft.AvatarFace = SaveFixture.Content.Get<AvatarCatalog>().FacesFor(BiologicalSex.Female).Last().Id;
            var form = new CreationFormViewModel(SaveFixture.Content, DashboardFixture.Keys());
            Assert.That(form.Problems(draft), Is.Empty);

            Character c = new CharacterCreator(SaveFixture.Content).Create(draft.ToRequest(7UL)).Character;
            Assert.That((c.Name.GivenName, c.Gender.Sex, c.Birth.Date.Year, c.Birth.Date.Month, c.Birth.Date.Day, c.Birth.Hour),
                Is.EqualTo(("Malee", BiologicalSex.Female, 1990, 2, 28, 6)));
            Assert.That(c.Residence.CityId, Is.EqualTo(draft.CityId));
            Assert.That(c.Family.BackgroundId, Is.EqualTo(draft.FamilyBackgroundId));
            Assert.That(c.Family.Parents.Single(p => p.Sex == BiologicalSex.Female).PersonalityId, Is.EqualTo(draft.MotherPersonalityId));
            Assert.That(AvatarFaces.Of(c), Is.EqualTo(draft.AvatarFace));
        }

        [Test]
        public void Problems_AreShownInThePlayersWords()
        {
            ILocalizer keys = DashboardFixture.Keys();
            var form = new CreationFormViewModel(SaveFixture.Content, keys);
            CreationDraft draft = CreationDraft.Random(SaveFixture.Content, 3UL);

            draft.Year = 2001;
            draft.Month = 2;
            draft.Day = 30;
            Assert.That(form.Problems(draft), Is.EqualTo(new[] { keys.Get(CreationIssue.BirthDateInvalid) }));

            draft.Day = 1;
            draft.GivenName = "   ";
            draft.Sex = BiologicalSex.Male;
            draft.AvatarFace = SaveFixture.Content.Get<AvatarCatalog>().FacesFor(BiologicalSex.Female).First().Id;
            Assert.That(form.Problems(draft), Is.EquivalentTo(new[] { keys.Get(CreationIssue.NameInvalid), keys.Get(CreationIssue.AvatarInvalid) }));
        }

        [Test]
        public void Options_ListFacesOfTheSex_AndFamiliesPoorestFirst()
        {
            var form = new CreationFormViewModel(SaveFixture.Content, DashboardFixture.Keys());
            Assert.That(form.Faces(BiologicalSex.Male, 4).Select(f => (f.FaceId, f.IsSelected)), Is.EqualTo(new[] { (3, false), (4, true) }));
            Assert.That(form.Faces(BiologicalSex.Male, null).All(f => f.Path.Contains("adult")), Is.True, "previewed as an adult");
            Assert.That(form.Families.First().Id, Is.EqualTo("poor"));
            Assert.That(form.Hours, Has.Count.EqualTo(24));
            Assert.That(form.Months, Has.Count.EqualTo(12));
            Assert.That(form.Years.First(), Is.GreaterThan(form.Years.Last()), "newest year first");
        }

        [Test]
        public void AHomeCountry_IsWhereEveryLifeIsBorn()
        {
            IContentRepository content = TestContent.Load(TestContent.Source()
                .Add("creation_settings.json", "{ 'birthYear': { 'min': 1980, 'max': 2020 }, 'parentAgeAtBirth': { 'min': 20, 'max': 40 }, 'homeCountry': 'japan' }"));
            Assert.That(content.Get<CreationSettings>().HomeCountryId, Is.EqualTo("japan"));
            for (ulong seed = 1; seed <= 30; seed++)
            {
                Character c = new CharacterCreator(content).Create(new CreationRequest(seed, CreationMode.Random)).Character;
                Assert.That(c.Residence.CountryId, Is.EqualTo("japan"));
            }
            Assert.That(new CreationFormViewModel(content, DashboardFixture.Keys()).HomeCountryId, Is.EqualTo("japan"));
            Assert.That(new CreationFormViewModel(SaveFixture.Content, DashboardFixture.Keys()).HomeCountryId, Is.Null, "without one the form offers countries");
        }
    }
}
