using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Events;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Plays whole lives with the real content (always taking the first available choice) and checks that
    /// life expectancy and causes of death look plausible. Fine balancing happens in the Balancing Simulator task.
    /// </summary>
    [TestFixture]
    public class LifeExpectancyTests
    {
        private const int Lives = 80;
        private const int MaxYearSkips = 130;

        [Test]
        public void WholeLives_EndAtPlausibleAges()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var creator = new CharacterCreator(content);
            var ages = new List<int>();
            var causes = new List<string>();

            for (int i = 0; i < Lives; i++)
            {
                CreationResult creation = creator.Create(new CreationRequest((ulong)(9000 + i * 31), CreationMode.Random));
                using (LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus()))
                {
                    for (int skip = 0; skip < MaxYearSkips && !life.IsOver; skip++)
                    {
                        life.Advance(TimeSkip.Year);
                        foreach (PendingEvent pending in life.Events.Inbox.ToList())
                        {
                            string choice = life.Events.GetChoices(life.Character, pending.InstanceId).FirstOrDefault(v => v.IsAvailable)?.Choice.Id;
                            if (choice != null && life.Character.IsAlive)
                            {
                                life.Events.Resolve(life.Character, pending.InstanceId, choice);
                            }
                        }
                    }
                    Assert.That(life.IsOver, Is.True, "every life ends by the maximum age");
                    ages.Add(life.Character.Death.AgeYears);
                    causes.Add(life.Character.Death.CauseId);
                }
            }

            double mean = ages.Average();
            TestContext.WriteLine($"Mean age at death {mean:0.0}, median {ages.OrderBy(a => a).ElementAt(Lives / 2)}, min {ages.Min()}, max {ages.Max()}");
            TestContext.WriteLine("Causes: " + string.Join(", ", causes.GroupBy(c => c).OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {g.Count()}")));
            Assert.That(mean, Is.InRange(60.0, 90.0));
            Assert.That(ages.Max(), Is.LessThanOrEqualTo(120));
            Assert.That(causes.Distinct().Count(), Is.GreaterThanOrEqualTo(3));
        }
    }
}
