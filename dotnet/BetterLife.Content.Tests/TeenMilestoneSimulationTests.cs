using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Events;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Real content: lives played from birth to 19 meet the teen milestones (chapter 1 of the user's 100 events)
    /// at about the chances the user set, raised by earlier milestones.
    /// </summary>
    [TestFixture]
    public class TeenMilestoneSimulationTests
    {
        private const int Lives = 80;
        private const int EndAge = 19;

        [Test]
        public void TeenMilestones_HappenAtAboutTheirChances()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            List<LifeEventDefinition> teen = content.Get<DefinitionCatalog<LifeEventDefinition>>().All
                .Where(e => e.Trigger == EventTrigger.Milestone && e.Category == "teen").ToList();
            Assert.That(teen.Count, Is.EqualTo(30));

            var counts = teen.ToDictionary(e => e.Id, e => 0);
            int reached = 0;
            for (int i = 0; i < Lives; i++)
            {
                CreationResult creation = new CharacterCreator(content).Create(new CreationRequest((ulong)(7300 + i * 29), CreationMode.Random));
                using (LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus(), RepoContent.Simulation))
                {
                    while (!life.IsOver && life.Character.AgeAt(life.TimeState.Clock.Now) < EndAge)
                    {
                        life.Advance(TimeSkip.Year);
                    }
                    if (life.IsOver)
                    {
                        continue;
                    }
                    reached++;
                    foreach (LifeEventDefinition e in teen.Where(e => life.Events.State.FindRecord(e.Id) != null))
                    {
                        counts[e.Id]++;
                    }
                }
            }

            Assert.That(reached, Is.GreaterThan(Lives * 9 / 10), "most lives reach 19");
            foreach (LifeEventDefinition e in teen.OrderBy(e => e.Id))
            {
                float rate = counts[e.Id] / (float)reached;
                TestContext.WriteLine($"{e.Id,-26} set {e.Milestone.Percent,3}%  seen {rate * 100f,5:0.0}%");
                if (e.Milestone.Percent >= 25f && e.Condition == null)
                {
                    Assert.That(rate, Is.GreaterThan(e.Milestone.Percent / 100f * 0.5f), e.Id + " happens far less than its chance");
                }
            }
            Assert.That(counts["teen.national_award"] / (float)reached, Is.LessThan(0.25f), "a 5% award stays rare even with boosts");
        }
    }
}
