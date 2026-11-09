using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Events;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.Vices;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Plays lives with the real content up to age 40 (career policy only, never choosing a vice on purpose) and checks
    /// that habits come from temptation alone at a plausible rate.
    /// </summary>
    [TestFixture]
    public class ViceSimulationTests
    {
        private const int Lives = 40;
        private const int Months = 40 * 12;
        private const float Hooked = 40f;

        [Test]
        public void Temptation_MakesSomeButNotAllLivesTryVices()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var creator = new CharacterCreator(content);
            var vices = content.Get<DefinitionCatalog<ViceDefinition>>();
            var tried = vices.All.ToDictionary(v => v.Id, v => 0);
            var hooked = vices.All.ToDictionary(v => v.Id, v => 0);
            int alive = 0, discovered = 0;

            for (int i = 0; i < Lives; i++)
            {
                CreationResult creation = creator.Create(new CreationRequest((ulong)(9100 + i * 11), CreationMode.Random));
                using (LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus()))
                {
                    for (int month = 0; month < Months && !life.IsOver; month++)
                    {
                        life.Advance(TimeSkip.Month);
                        if (!life.IsOver)
                        {
                            CareerSimulationTests.Decide(life);
                        }
                    }
                    if (life.IsOver)
                    {
                        continue;
                    }
                    alive++;
                    foreach (ViceState state in life.Character.Vices.States)
                    {
                        Assert.That(state.Addiction, Is.InRange(0f, 100f));
                        Assert.That(state.Tolerance, Is.InRange(0f, 100f));
                        tried[state.ViceId] += state.EverUsed ? 1 : 0;
                        hooked[state.ViceId] += state.Addiction >= Hooked ? 1 : 0;
                        discovered += state.Discovered ? 1 : 0;
                    }
                }
            }

            TestContext.WriteLine($"Alive at 40: {alive}; family discoveries {discovered}");
            foreach (ViceDefinition vice in vices.All)
            {
                TestContext.WriteLine($"  {vice.Id}: tried {tried[vice.Id]}, hooked (addiction >= {Hooked}) {hooked[vice.Id]}");
            }
            Assert.That(alive, Is.GreaterThan(Lives / 2));
            Assert.That(tried.Values.Sum(), Is.GreaterThan(0), "temptation alone leads some lives to try vices");
            Assert.That(hooked.Values.Max(), Is.LessThan(alive / 2), "most lives that never choose a vice do not end up hooked");
        }
    }
}
