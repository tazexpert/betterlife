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
    /// Real content: lives from birth to 75 meet a variety of random events (the user's RE-01..RE-30 and the older ones)
    /// instead of the same few over and over.
    /// </summary>
    [TestFixture]
    public class RandomEventVarietySimulationTests
    {
        private const int Lives = 16;
        private const int EndAge = 75;
        private const float MaxShareOfOneEvent = 0.2f;

        // Need a partner, an ex or children, which the simulated player never arranges.
        private static readonly HashSet<string> NeedsFamily = new HashSet<string>
        {
            "re.the_ex", "re.forgot_anniversary", "re.grandchild_startup", "re.family_trip", "re.inheritance_drama",
        };

        [Test]
        public void RandomEvents_AreVaried_AndEveryUserEventCanHappen()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var catalog = content.Get<DefinitionCatalog<LifeEventDefinition>>();
            var counts = new Dictionary<string, int>();
            int years = 0;
            for (int i = 0; i < Lives; i++)
            {
                CreationResult creation = new CharacterCreator(content).Create(new CreationRequest(3300UL + (ulong)(i * 31), CreationMode.Random));
                using (LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus(), RepoContent.Simulation))
                {
                    while (!life.IsOver && life.Character.AgeAt(life.TimeState.Clock.Now) < EndAge)
                    {
                        CareerSimulationTests.Decide(life);
                        life.Advance(TimeSkip.Month);
                    }
                    years += life.Character.AgeAt(life.TimeState.Clock.Now);
                    foreach (EventHistoryEntry entry in life.Events.State.History.Where(h => catalog.Get(h.EventId).Trigger == EventTrigger.Random))
                    {
                        counts[entry.EventId] = counts.TryGetValue(entry.EventId, out int n) ? n + 1 : 1;
                    }
                }
            }

            int total = counts.Values.Sum();
            TestContext.WriteLine($"{total} random events in {years} years ({total / (float)years:0.0}/year)");
            foreach (KeyValuePair<string, int> pair in counts.OrderByDescending(p => p.Value))
            {
                TestContext.WriteLine($"{pair.Key,-28} {pair.Value,4}  {pair.Value * 100f / total,5:0.0}%");
            }
            foreach (LifeEventDefinition e in catalog.All.Where(e => e.Id.StartsWith("re.") && e.Trigger == EventTrigger.Random && !NeedsFamily.Contains(e.Id)))
            {
                Assert.That(counts.ContainsKey(e.Id), Is.True, e.Id + " never happens");
            }
            Assert.That(counts.Values.Max() / (float)total, Is.LessThan(MaxShareOfOneEvent), "no single event dominates");
        }
    }
}
