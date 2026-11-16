using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Localization;
using BetterLife.Core.Relationships;
using BetterLife.Core.Sessions;
using BetterLife.Core.Tests.Dashboard;
using BetterLife.Core.Tests.Saving;
using BetterLife.Core.Time;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.SubScreens
{
    /// <summary>Family roles by sex and age, the sex shown for everyone, and siblings never being the same age.</summary>
    [TestFixture]
    public class FamilyRoleTests
    {
        [Test]
        public void Roles_FollowSexAndAge()
        {
            ILocalizer keys = DashboardFixture.Keys();
            using (LifeSession life = SaveFixture.NewLife())
            {
                Character c = life.Character;
                GameDate born = c.Birth.Week;
                string Role(RelationshipType type, BiologicalSex sex, int weeksFromBirth)
                {
                    Relationship r = c.Relationships.Add(id => new Npc(id, new PersonName("X", "Y"), sex, born.AddWeeks(weeksFromBirth), new Genome(), new string[0]),
                        type, 50f, born);
                    return PeopleViewModel.RoleName(r, c.Relationships.Npc(r.NpcId), c, keys);
                }

                Assert.That(Role(RelationshipType.Parent, BiologicalSex.Male, -30 * 52), Is.EqualTo("relationship.role.father"));
                Assert.That(Role(RelationshipType.Parent, BiologicalSex.Female, -28 * 52), Is.EqualTo("relationship.role.mother"));
                Assert.That(Role(RelationshipType.Sibling, BiologicalSex.Male, -3 * 52), Is.EqualTo("relationship.role.older_brother"));
                Assert.That(Role(RelationshipType.Sibling, BiologicalSex.Female, 2 * 52), Is.EqualTo("relationship.role.younger_sister"));
                Assert.That(Role(RelationshipType.Sibling, BiologicalSex.Female, 0), Is.EqualTo("relationship.role.twin_sister"));
                Assert.That(Role(RelationshipType.Child, BiologicalSex.Male, 30 * 52), Is.EqualTo("relationship.role.son"));
                Assert.That(Role(RelationshipType.Spouse, BiologicalSex.Female, 0), Is.EqualTo("relationship.role.wife"));
                Assert.That(Role(RelationshipType.Friend, BiologicalSex.Female, 0), Is.EqualTo("relationship.type.friend"), "others keep their type");

                PersonRow row = PeopleViewModel.Build(life, keys).SelectMany(g => g.People).First();
                Assert.That(row.Detail, Is.EqualTo(PeopleViewModel.RowDetailKey), "the row template now carries {sex}");
                Assert.That(PeopleViewModel.SexName(c.Relationships.Npcs.First(), keys), Does.StartWith(PeopleViewModel.SexPrefix));
            }
        }

        [Test]
        public void SiblingsAtBirth_AreNeverTheSameAge()
        {
            int checkedSiblings = 0;
            for (ulong seed = 1; seed <= 40; seed++)
            {
                using (LifeSession life = SaveFixture.NewLife(seed))
                {
                    GameDate born = life.Character.Birth.Week;
                    foreach (Relationship sibling in life.Character.Relationships.All.Where(r => r.Type == RelationshipType.Sibling))
                    {
                        int gap = System.Math.Abs(life.Character.Relationships.Npc(sibling.NpcId).BornOn.TotalWeeks - born.TotalWeeks);
                        Assert.That(gap, Is.GreaterThanOrEqualTo(GameCalendar.WeeksPerYear), $"seed {seed}: a sibling only {gap} weeks apart");
                        checkedSiblings++;
                    }
                }
            }
            Assert.That(checkedSiblings, Is.GreaterThan(10), "enough siblings were generated to check");
        }
    }
}
