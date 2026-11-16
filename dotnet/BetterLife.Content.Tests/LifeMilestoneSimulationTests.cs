using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Events;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Relationships;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Real content: lives played from birth meet the user's milestones (prequel ages 3-12, chapter 1 ages 13-18, chapter 2 ages 19-25)
    /// at about the chances the user set, raised by earlier milestones.
    /// </summary>
    [TestFixture]
    public class LifeMilestoneSimulationTests
    {
        private const int Lives = 80;
        private const int DatingAge = 26;
        private const string ChatId = "chat";
        private const float LowestShareOfChance = 0.5f;
        private const float CheckedFromPercent = 25f;

        [Test]
        public void KidMilestones_HappenAtAboutTheirChances()
        {
            List<HashSet<string>> lives = Play("kid", 20, 13, 5100, out List<LifeEventDefinition> kid);
            Report(kid, lives);
            Assert.That(lives.Count(l => l.Contains("kid.famous_primary") && l.Contains("kid.missed_primary")), Is.EqualTo(0),
                "a child cannot both pass and miss the famous school");
        }

        [Test]
        public void TeenMilestones_HappenAtAboutTheirChances()
        {
            List<HashSet<string>> lives = Play("teen", 30, 19, 7300, out List<LifeEventDefinition> teen);
            Report(teen, lives);
            Assert.That(Rate(lives, "teen.national_award"), Is.LessThan(0.25f), "a 5% award stays rare even with boosts");
        }

        [Test]
        public void YoungAdultMilestones_HappenAtAboutTheirChances()
        {
            List<HashSet<string>> lives = Play("young", 20, 26, 9100, out List<LifeEventDefinition> young, CareerSimulationTests.Decide);
            Report(young, lives);
            foreach (LifeEventDefinition e in young)
            {
                Assert.That(lives.Any(l => l.Contains(e.Id)), Is.True, e.Id + " never happens to students or workers");
            }
        }

        [Test]
        public void AdultMilestones_HappenAtAboutTheirChances()
        {
            List<HashSet<string>> lives = Play("adult", 23, 46, 11300, out List<LifeEventDefinition> adult, DecideAndDate, 40);
            Report(adult, lives);
            foreach (LifeEventDefinition e in adult)
            {
                Assert.That(lives.Any(l => l.Contains(e.Id)), Is.True, e.Id + " never happens");
            }
            int engaged = lives.Count(l => l.Contains("adult.engaged"));
            TestContext.WriteLine($"engaged {engaged}, wedding {Rate(lives, "adult.wedding") * lives.Count}, school {Rate(lives, "adult.child_school") * lives.Count}");
            Assert.That(lives.Count(l => l.Contains("adult.wedding")), Is.GreaterThan(engaged / 2), "most engagements end in a wedding");
            Assert.That(lives.Any(l => l.Contains("adult.child_school")), Is.True, "children start school");
        }

        [Test]
        public void OldAgeMilestones_HappenAndChangeRealState()
        {
            List<HashSet<string>> lives = Play("old", 23, 80, 13700, out List<LifeEventDefinition> old, DecideAndDate, 30, 0.3f);
            Report(old, lives);
            // The simulated player always finds a new partner, so the events that need a single character are not checked here.
            var needsSingle = new HashSet<string> { "old.late_love", "old.chosen_solitude" };
            foreach (LifeEventDefinition e in old.Where(e => !needsSingle.Contains(e.Id)))
            {
                Assert.That(lives.Any(l => l.Contains(e.Id)), Is.True, e.Id + " never happens");
            }
            Assert.That(lives.All(l => l.Contains("old.retirement")), Is.True, "everyone who reaches 60 meets retirement");
            Assert.That(lives.Any(l => l.Contains("old.child_graduates")), Is.True, "the first child graduates about 22 years later");
        }

        [Test]
        public void OldAgeMilestones_ForSingles_Happen()
        {
            List<HashSet<string>> lives = Play("old", 23, 80, 15100, out List<LifeEventDefinition> old, CareerSimulationTests.Decide, 30, 0.3f);
            Assert.That(lives.Any(l => l.Contains("old.late_love")), Is.True, "singles can find love again");
            Assert.That(lives.Any(l => l.Contains("old.chosen_solitude")), Is.True, "singles can choose to stay alone");
            TestContext.WriteLine($"late love {Rate(lives, "old.late_love") * 100f:0}%, solitude {Rate(lives, "old.chosen_solitude") * 100f:0}%");
        }

        /// <summary>
        /// The career player who also dates: meets a partner at 26 when single and chats with them every month
        /// (a real player tends the relationship; neglected partners leave).
        /// </summary>
        private static void DecideAndDate(LifeSession life)
        {
            CareerSimulationTests.Decide(life);
            var people = life.Character.Relationships;
            Relationship partner = people.Closest(RelationshipType.Spouse) ?? people.Closest(RelationshipType.Partner);
            if (partner == null && life.Character.AgeAt(life.TimeState.Clock.Now) >= DatingAge)
            {
                life.Bus.Publish(new MeetRequestedEvent(RelationshipType.Partner, life.TimeState.Clock.Now));
                return;
            }
            if (partner != null && life.Relationships.Quote(partner.NpcId, ChatId).Available)
            {
                life.Relationships.Interact(partner.NpcId, ChatId);
            }
        }

        private static List<HashSet<string>> Play(string category, int expected, int endAge, ulong seedBase, out List<LifeEventDefinition> events,
            System.Action<LifeSession> decide = null, int lives = Lives, float mustReach = 0.9f)
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            events = content.Get<DefinitionCatalog<LifeEventDefinition>>().All
                .Where(e => e.Trigger == EventTrigger.Milestone && e.Category == category).ToList();
            Assert.That(events.Count, Is.EqualTo(expected));

            var played = new List<HashSet<string>>();
            for (int i = 0; i < lives; i++)
            {
                CreationResult creation = new CharacterCreator(content).Create(new CreationRequest(seedBase + (ulong)(i * 29), CreationMode.Random));
                using (LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus(), RepoContent.Simulation))
                {
                    while (!life.IsOver && life.Character.AgeAt(life.TimeState.Clock.Now) < endAge)
                    {
                        if (decide == null)
                        {
                            life.Advance(TimeSkip.Year);
                            continue;
                        }
                        decide(life);
                        life.Advance(TimeSkip.Month);
                    }
                    if (!life.IsOver)
                    {
                        played.Add(new HashSet<string>(content.Get<DefinitionCatalog<LifeEventDefinition>>().All
                            .Where(e => e.Category == category && life.Events.State.FindRecord(e.Id) != null).Select(e => e.Id)));
                    }
                }
            }
            Assert.That(played.Count, Is.GreaterThanOrEqualTo((int)(lives * mustReach)), "enough lives reach the end of the chapter");
            return played;
        }

        private static void Report(List<LifeEventDefinition> events, List<HashSet<string>> lives)
        {
            foreach (LifeEventDefinition e in events.OrderBy(e => e.Id))
            {
                float rate = Rate(lives, e.Id);
                TestContext.WriteLine($"{e.Id,-26} set {e.Milestone.Percent,3}%  seen {rate * 100f,5:0.0}%");
                if (e.Milestone.Percent >= CheckedFromPercent && e.Condition == null)
                {
                    Assert.That(rate, Is.GreaterThan(e.Milestone.Percent / 100f * LowestShareOfChance), e.Id + " happens far less than its chance");
                }
            }
        }

        private static float Rate(List<HashSet<string>> lives, string id)
        {
            return lives.Count(l => l.Contains(id)) / (float)lives.Count;
        }
    }
}
