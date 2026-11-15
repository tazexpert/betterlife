using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Achievements;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Events;
using BetterLife.Core.Legacy;
using BetterLife.Core.Relationships;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Real content: families played for several generations (each heir is the eldest living child), with the real
    /// achievements checked every year.
    /// </summary>
    [TestFixture]
    public class LegacySimulationTests
    {
        private const int Families = 6;
        private const int MaxGenerations = 3;
        private const int MaxYearsPerLife = 110;
        private const int ParentAge = 30;
        private const float ChildCloseness = 70f;

        [Test]
        public void RealFamilies_CarryOnForGenerations()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var legacy = new LegacyService(content);
            var tracker = new AchievementTracker(content.Get<DefinitionCatalog<AchievementDefinition>>(), new AchievementStore(new InMemorySaveStorage()),
                new SystemRealTimeClock());
            var generations = new List<int>();
            for (int f = 0; f < Families; f++)
            {
                CreationResult creation = new CharacterCreator(content).Create(new CreationRequest((ulong)(9100 + f * 37), CreationMode.Random));
                LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus(), RepoContent.Simulation);
                while (true)
                {
                    bool hadChildren = false;
                    for (int year = 0; year < MaxYearsPerLife && !life.IsOver; year++)
                    {
                        life.Advance(TimeSkip.Year);
                        tracker.Evaluate(life);
                        if (!hadChildren && !life.IsOver && life.Character.AgeAt(life.TimeState.Clock.Now) >= ParentAge)
                        {
                            // Having children is a player decision (an interaction); here the "player" has two at 30.
                            HaveChildren(life, 2);
                            hadChildren = true;
                        }
                    }
                    Assert.That(life.IsOver, Is.True, $"family {f} generation {life.Character.Generation} never died");
                    var heirs = legacy.Heirs(life);
                    if (heirs.Count == 0 || life.Character.Generation >= MaxGenerations)
                    {
                        break;
                    }
                    int eldest = heirs.OrderBy(r => life.Character.Relationships.Npc(r.NpcId).BornOn).First().NpcId;
                    LifeSession next = legacy.ContinueAs(life, eldest, new EventBus(), out InheritanceReport report, RepoContent.Simulation);
                    Assert.That(report.Received, Is.GreaterThanOrEqualTo(0m));
                    Assert.That(next.Lineage.Ancestors.Count, Is.EqualTo(life.Character.Generation));
                    life.Dispose();
                    life = next;
                }
                generations.Add(life.Character.Generation);
                life.Dispose();
            }
            TestContext.WriteLine("Generations reached: " + string.Join(", ", generations)
                + " · achievements unlocked: " + string.Join(", ", tracker.Profile.Unlocked.Select(u => u.Id)));
            Assert.That(generations.Max(), Is.EqualTo(MaxGenerations), "families with children carry on");
            Assert.That(tracker.Profile.Has("adulthood"), Is.True);
        }

        private static void HaveChildren(LifeSession life, int count)
        {
            Character c = life.Character;
            GameDate now = life.TimeState.Clock.Now;
            for (int i = 0; i < count; i++)
            {
                var genome = new Genome();
                foreach (var gene in c.Genome.Values)
                {
                    genome.Set(gene.Key, gene.Value);
                }
                string given = "Child" + (char)('A' + i);
                c.Relationships.Add(id => new Npc(id, new PersonName(given, c.Name.FamilyName), i % 2 == 0 ? BiologicalSex.Female : BiologicalSex.Male,
                    now.AddWeeks(-i), genome, new string[0]), RelationshipType.Child, ChildCloseness, now);
            }
        }
    }
}
