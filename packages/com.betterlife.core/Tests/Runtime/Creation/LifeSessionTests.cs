using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Events;
using BetterLife.Core.Sessions;
using BetterLife.Core.Tests.Content;
using BetterLife.Core.Tests.LifeEvents;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Creation
{
    /// <summary>Tests for <see cref="LifeSessionFactory"/>.</summary>
    [TestFixture]
    public class LifeSessionTests
    {
        [Test]
        public void NewSession_StartsAtBirth_AndRunsEveryStandardPhase()
        {
            IContentRepository content = TestContent.Load(TestContent.Source("{ 'type': 'events', 'path': 'events/test.json' }")
                .Add("events/test.json", EventFixture.Events(EventFixture.Simple("test.common"))));
            CreationResult creation = new CharacterCreator(content).Create(new CreationRequest(2026UL, CreationMode.Random));

            using (LifeSession session = LifeSessionFactory.CreateNew(content, creation, new EventBus()))
            {
                Assert.That(session.TimeState.Clock.Now, Is.EqualTo(session.Character.Birth.Week));
                Assert.That(session.Seed, Is.EqualTo(2026UL));
                Assert.That(session.Time.Phases, Has.Some.Matches<ITickPhase>(p => p.Name == "event_roll"));
                Assert.That(session.Time.Phases, Has.Some.Matches<ITickPhase>(p => p.Name == "karmic_debt"));

                AdvanceResult result = session.Advance(TimeSkip.Year);

                Assert.That(result.To.IsYearStart, Is.True);
                Assert.That(session.Character.History.LastRecordedWeek, Is.EqualTo(result.To));
            }
        }
    }
}
