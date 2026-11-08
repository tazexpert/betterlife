using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Creates many lives from the real content and plays one for twenty years.
    /// </summary>
    [TestFixture]
    public class CreationContentTests
    {
        private const int Lives = 500;
        private const int Years = 20;

        private IContentRepository _content;

        [OneTimeSetUp]
        public void Load()
        {
            _content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
        }

        [Test]
        public void ManyRandomLives_AreValid_AndVaried()
        {
            var creator = new CharacterCreator(_content);
            var lives = Enumerable.Range(0, Lives).Select(i => creator.Create(new CreationRequest((ulong)(i * 7919 + 13), CreationMode.Random)).Character).ToList();

            foreach (Character c in lives)
            {
                foreach (StatDefinition stat in c.Catalogs.Stats.Definitions)
                {
                    Assert.That(c.Stats.GetValue(stat.Id), Is.InRange(0f, 100f));
                }
            }
            double thai = lives.Count(c => c.Residence.CountryId == "thailand") / (double)Lives;
            TestContext.WriteLine($"Thailand {thai:P0}, backgrounds: " + string.Join(", ", lives.GroupBy(c => c.Family.BackgroundId).OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {g.Count()}")));
            TestContext.WriteLine($"Mean wealth {lives.Average(c => c.Stats.GetValue(StatIds.Wealth)):0.0}, mean IQ {lives.Average(c => c.Stats.GetValue(StatIds.Iq)):0.0}, with traits {lives.Count(c => c.Traits.Instances.Count > 0)}");
            Assert.That(thai, Is.EqualTo(0.9).Within(0.05));
            Assert.That(lives.Select(c => c.Family.BackgroundId).Distinct().Count(), Is.GreaterThanOrEqualTo(5));
            Assert.That(lives.Select(c => c.Name.GivenName).Distinct().Count(), Is.GreaterThan(20));
        }

        [Test]
        public void ChallengeCode_RecreatesTheSameLife()
        {
            var creator = new CharacterCreator(_content);
            string code = creator.Create(new CreationRequest(424242UL, CreationMode.Random)).ChallengeCode;

            Assert.That(ChallengeCode.TryDecode(code, out ulong seed), Is.True);
            Character a = creator.Create(new CreationRequest(seed, CreationMode.Challenge)).Character;
            Character b = creator.Create(new CreationRequest(424242UL, CreationMode.Challenge)).Character;
            Assert.That((a.Id, a.Name.GivenName, a.Birth.Date, a.Residence.CityId), Is.EqualTo((b.Id, b.Name.GivenName, b.Birth.Date, b.Residence.CityId)));
        }

        [Test]
        public void ALife_PlaysForTwentyYears()
        {
            CreationResult creation = new CharacterCreator(_content).Create(new CreationRequest(2026UL, CreationMode.Random));
            using (LifeSession session = LifeSessionFactory.CreateNew(_content, creation, new EventBus()))
            {
                int skips = 0;
                while (session.Character.AgeAt(session.TimeState.Clock.Now) < Years && skips++ < Years * 60)
                {
                    session.Advance(TimeSkip.Year);
                    foreach (var pending in session.Events.Inbox.ToList())
                    {
                        string choice = session.Events.GetChoices(session.Character, pending.InstanceId).FirstOrDefault(v => v.IsAvailable)?.Choice.Id;
                        if (choice != null)
                        {
                            session.Events.Resolve(session.Character, pending.InstanceId, choice);
                        }
                    }
                }

                Character c = session.Character;
                TestContext.WriteLine($"{c.Name.GivenName} {c.Name.FamilyName}, born {c.Birth.Date} in {c.Residence.CityId}, family {c.Family.BackgroundId}, age {c.AgeAt(session.TimeState.Clock.Now)}, events {session.EventState.History.Count}");
                Assert.That(c.AgeAt(session.TimeState.Clock.Now), Is.GreaterThanOrEqualTo(Years));
                Assert.That(session.EventState.History.Count, Is.GreaterThan(Years));
            }
        }
    }
}
