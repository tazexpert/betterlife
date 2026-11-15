using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Relationships;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Plays lives with the real content up to age 40 with the career policy plus a simple romance policy
    /// (date the closest friend, propose when close, try for up to two children) and checks the relationship system.
    /// </summary>
    [TestFixture]
    public class RelationshipSimulationTests
    {
        private const int Lives = 30;
        private const int Months = 40 * 12;
        private const int DatingAge = 20;
        private const int WantedChildren = 2;

        [Test]
        public void Lives_HaveFamiliesFriendsAndPartners()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var creator = new CharacterCreator(content);
            int alive = 0, married = 0, parents = 0, children = 0, friends = 0, everDated = 0;
            var scores = new List<float>();
            int maxKept = content.Get<RelationshipSettings>().MaxRelationships;

            for (int i = 0; i < Lives; i++)
            {
                CreationResult creation = creator.Create(new CreationRequest((ulong)(8300 + i * 7), CreationMode.Random));
                using (LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus()))
                {
                    Assert.That(life.Character.Relationships.Living(RelationshipType.Parent).Count(), Is.EqualTo(2), "everyone starts with parents");
                    bool dated = false;
                    for (int month = 0; month < Months && !life.IsOver; month++)
                    {
                        life.Advance(TimeSkip.Month);
                        if (life.IsOver)
                        {
                            break;
                        }
                        CareerSimulationTests.Decide(life);
                        dated |= Romance(life);
                    }
                    if (life.IsOver)
                    {
                        continue;
                    }
                    CharacterRelationships people = life.Character.Relationships;
                    alive++;
                    everDated += dated ? 1 : 0;
                    married += people.Living(RelationshipType.Spouse).Any() ? 1 : 0;
                    int kids = people.Living(RelationshipType.Child).Count();
                    parents += kids > 0 ? 1 : 0;
                    children += kids;
                    friends += people.Living(RelationshipType.Friend).Count();
                    scores.Add(life.Character.Stats.GetValue(StatIds.Relationships));
                    Assert.That(people.All.Count(r => !RelationshipNames.IsFamily(r.Type) && !RelationshipNames.IsRomantic(r.Type)), Is.LessThanOrEqualTo(maxKept));
                    Assert.That(people.All.Select(r => r.Value), Is.All.InRange(Relationship.Min, Relationship.Max));
                }
            }

            TestContext.WriteLine($"Alive at 40: {alive}; ever dated {everDated}, married {married}, parents {parents} ({children} children), "
                + $"friends per life {(alive == 0 ? 0 : friends / (float)alive):0.0}, Relationships stat {scores.Average():0.0} (min {scores.Min():0}, max {scores.Max():0})");
            Assert.That(alive, Is.GreaterThan(Lives / 2));
            Assert.That(everDated, Is.GreaterThan(alive / 2), "a social player finds partners");
            Assert.That(married, Is.GreaterThan(0));
            Assert.That(parents, Is.GreaterThan(0));
            Assert.That(scores, Is.All.InRange(0f, 100f));
        }

        private static bool Romance(LifeSession life)
        {
            var character = life.Character;
            if (character.AgeAt(life.TimeState.Clock.Now) < DatingAge)
            {
                return false;
            }
            CharacterRelationships people = character.Relationships;
            Relationship spouse = people.Closest(RelationshipType.Spouse);
            Relationship partner = people.Closest(RelationshipType.Partner);
            if (spouse != null)
            {
                if (people.Living(RelationshipType.Child).Count() < WantedChildren)
                {
                    life.Relationships.Interact(spouse.NpcId, "try_for_baby");
                }
                return true;
            }
            if (partner != null)
            {
                life.Relationships.Interact(partner.NpcId, partner.Value >= 60f ? "propose" : "deep_talk");
                return true;
            }
            Relationship crush = people.Living().Where(r => r.Type == RelationshipType.Friend || r.Type == RelationshipType.Colleague)
                .OrderByDescending(r => r.Value).FirstOrDefault();
            if (crush != null)
            {
                life.Relationships.Interact(crush.NpcId, crush.Value >= 40f ? "ask_out" : "chat");
            }
            return false;
        }
    }
}
