using System.Linq;
using BetterLife.Core.Avatars;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Events;
using BetterLife.Core.Legacy;
using BetterLife.Core.Relationships;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using BetterLife.Core.Tests.Dashboard;
using BetterLife.Core.Tests.Legacy;
using BetterLife.Core.Tests.Saving;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Avatars
{
    /// <summary>Tests for task 27: avatar faces by sex and age band, the picker, saving and heirs.</summary>
    [TestFixture]
    public class AvatarTests
    {
        private static AvatarCatalog Catalog() => SaveFixture.Content.Get<AvatarCatalog>();

        [Test]
        public void Paths_FollowTheAgeBands_AndTheFilePrefixes()
        {
            AvatarCatalog avatars = Catalog();
            Assert.That(avatars.PathFor(3, 0), Is.EqualTo("BetterLife/Avatars/young/young_03"));
            Assert.That(avatars.PathFor(20, 17), Is.EqualTo("BetterLife/Avatars/young/young_20"));
            Assert.That(avatars.PathFor(20, 18), Is.EqualTo("BetterLife/Avatars/adult/images/adult_20"), "a band may sit in a subfolder");
            Assert.That(avatars.PathFor(4, 120), Is.EqualTo("BetterLife/Avatars/adult/images/adult_04"), "the last band has no upper age");
        }

        [Test]
        public void Faces_MatchTheSex_AreStable_AndAChoiceWins()
        {
            AvatarCatalog avatars = Catalog();
            Assert.That(avatars.FacesFor(BiologicalSex.Male).Select(f => f.Id), Is.EqualTo(new[] { 3, 4 }));
            for (int i = 0; i < 20; i++)
            {
                int face = avatars.FaceFor(BiologicalSex.Female, null, "life:" + i);
                Assert.That(new[] { 20, 21 }, Does.Contain(face), "a derived face has the person's sex");
                Assert.That(avatars.FaceFor(BiologicalSex.Female, null, "life:" + i), Is.EqualTo(face), "and never changes");
            }
            Assert.That(avatars.FaceFor(BiologicalSex.Male, 4, "x"), Is.EqualTo(4));
            Assert.That(avatars.FaceFor(BiologicalSex.Male, 21, "x"), Is.Not.EqualTo(21), "a face of the other sex is not used");
        }

        [Test]
        public void ThePlayer_PicksAFace_ThatIsSaved()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 20);
                var options = AvatarPickerViewModel.Build(life);
                BiologicalSex sex = life.Character.Gender.Sex;
                Assert.That(options.Select(o => o.FaceId), Is.EqualTo(Catalog().FacesFor(sex).Select(f => f.Id)), "only faces of the player's sex");
                Assert.That(options.Count(o => o.IsSelected), Is.EqualTo(1));
                Assert.That(options.All(o => o.Path.Contains("adult")), Is.True, "drawn at the player's age");

                AvatarOption other = options.First(o => !o.IsSelected);
                Assert.That(AvatarPickerViewModel.Choose(life, other.FaceId), Is.True);
                int otherSex = Catalog().FacesFor(sex == BiologicalSex.Male ? BiologicalSex.Female : BiologicalSex.Male)[0].Id;
                Assert.That(AvatarPickerViewModel.Choose(life, otherSex), Is.False);
                Assert.That(AvatarFaces.Of(life.Character), Is.EqualTo(other.FaceId));
                Assert.That(DashboardViewModel.Build(life, DashboardFixture.Keys()).AvatarPath, Does.EndWith(other.FaceId.ToString("00")));

                var storage = new InMemorySaveStorage();
                var saves = new SaveService(storage, new SaveSerializer(), SaveFixture.Content);
                saves.Save(life, "slot_1");
                using (LifeSession loaded = saves.Load("slot_1", new EventBus()).Session)
                {
                    Assert.That(loaded.Character.AvatarFace, Is.EqualTo(other.FaceId));
                    Assert.That(loaded.Character.Relationships.Npcs.OrderBy(n => n.Id).Select(n => (n.Id, AvatarFaces.Of(loaded.Character, n))).ToList(),
                        Is.EqualTo(life.Character.Relationships.Npcs.OrderBy(n => n.Id).Select(n => (n.Id, AvatarFaces.Of(life.Character, n))).ToList()),
                        "people keep their faces after loading");
                }
            }
        }

        [Test]
        public void People_ShowTheirFace_AtTheirAge()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                Npc parent = life.Character.Relationships.Npcs.First(n => life.Character.Relationships.With(n.Id).Type == RelationshipType.Parent);
                string path = AvatarPaths.Of(life.Character, parent, life.TimeState.Clock.Now);
                Assert.That(path, Does.Contain("adult"), "parents are adults");
                Assert.That(path, Does.EndWith(AvatarFaces.Of(life.Character, parent).ToString("00")));
                Assert.That(Catalog().FacesFor(parent.Sex).Select(f => f.Id), Does.Contain(AvatarFaces.Of(life.Character, parent)));
            }
        }

        [Test]
        public void AnHeir_KeepsTheirFace_AndTheirFamilyKeepsTheirs()
        {
            using (LifeSession parent = LegacyTests.FinishedLife(out int heirId, out int siblingId))
            {
                Character dead = parent.Character;
                int heirFace = AvatarFaces.Of(dead, dead.Relationships.Npc(heirId));
                int siblingFace = AvatarFaces.Of(dead, dead.Relationships.Npc(siblingId));
                int parentFace = AvatarFaces.Of(dead);
                using (LifeSession heir = new LegacyService(SaveFixture.Content).ContinueAs(parent, heirId, new EventBus(), out _))
                {
                    Character c = heir.Character;
                    Assert.That(AvatarFaces.Of(c), Is.EqualTo(heirFace), "the child plays on with the same face");
                    Npc sibling = c.Relationships.Npcs.Single(n => n.Name.GivenName == "Sibling");
                    Npc mother = c.Relationships.Npcs.Single(n => n.Name.GivenName == dead.Name.GivenName);
                    Assert.That(AvatarFaces.Of(c, sibling), Is.EqualTo(siblingFace));
                    Assert.That(AvatarFaces.Of(c, mother), Is.EqualTo(parentFace));
                }
            }
        }
    }
}
