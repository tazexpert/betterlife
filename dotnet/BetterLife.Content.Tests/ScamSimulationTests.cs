using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Events;
using BetterLife.Core.Scams;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Plays real-content lives to age 75 with the career policy and checks that scams happen, hit older people more,
    /// and never drive money below zero.
    /// </summary>
    [TestFixture]
    public class ScamSimulationTests
    {
        private const int Lives = 30;
        private const int Months = 75 * 12;
        private const int ElderAge = 60;

        [Test]
        public void Scams_TargetEveryone_ButOlderPeopleMore()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var creator = new CharacterCreator(content);
            int lives = 0, victims = 0;
            long youngerMonths = 0, elderMonths = 0;
            int youngerScams = 0, elderScams = 0;
            var lost = new List<decimal>();
            var byScam = new Dictionary<string, (int Count, decimal Lost)>();

            for (int i = 0; i < Lives; i++)
            {
                CreationResult creation = creator.Create(new CreationRequest((ulong)(5200 + i * 23), CreationMode.Random));
                using (LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus()))
                {
                    life.Bus.Subscribe<ScammedEvent>(e =>
                    {
                        byScam.TryGetValue(e.Incident.ScamId, out var total);
                        byScam[e.Incident.ScamId] = (total.Count + 1, total.Lost + e.Incident.Lost);
                        if (life.Character.AgeAt(e.Week) >= ElderAge)
                        {
                            elderScams++;
                        }
                        else
                        {
                            youngerScams++;
                        }
                    });
                    for (int month = 0; month < Months && !life.IsOver; month++)
                    {
                        life.Advance(TimeSkip.Month);
                        if (life.IsOver)
                        {
                            break;
                        }
                        CareerSimulationTests.Decide(life);
                        int age = life.Character.AgeAt(life.TimeState.Clock.Now);
                        if (age >= ElderAge)
                        {
                            elderMonths++;
                        }
                        else if (age >= 18)
                        {
                            youngerMonths++;
                        }
                        Assert.That(life.Character.Wallet.Cash, Is.GreaterThanOrEqualTo(0m));
                    }
                    lives++;
                    victims += life.Character.Scams.Victimized > 0 ? 1 : 0;
                    lost.Add(life.Character.Scams.NetLost);
                }
            }

            double youngerRate = youngerScams * 12.0 / System.Math.Max(1, youngerMonths);
            double elderRate = elderScams * 12.0 / System.Math.Max(1, elderMonths);
            List<decimal> sorted = lost.OrderBy(v => v).ToList();
            TestContext.WriteLine($"Lives {lives}; ever scammed {victims}; scams per year: adults 18-59 {youngerRate:0.00}, 60+ {elderRate:0.00}; "
                + $"lifetime loss median {sorted[sorted.Count / 2]:N0}, max {sorted.Last():N0}");
            foreach (var pair in byScam.OrderByDescending(p => p.Value.Lost))
            {
                TestContext.WriteLine($"  {pair.Key}: {pair.Value.Count} times, {pair.Value.Lost:N0} baht");
            }
            Assert.That(victims, Is.GreaterThan(0));
            Assert.That(elderRate, Is.GreaterThan(youngerRate), "older people are targeted more");
            Assert.That(sorted, Is.All.GreaterThanOrEqualTo(0m));
        }
    }
}
