using System;
using BetterLife.Core.Events;
using BetterLife.Core.Saving;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Saving
{
    /// <summary>Tests for <see cref="UndoService"/>.</summary>
    [TestFixture]
    public class UndoServiceTests
    {
        private FakeClock _clock;
        private InMemorySaveStorage _profile;

        [SetUp]
        public void SetUp()
        {
            _clock = new FakeClock { Today = new DateTime(2026, 11, 9) };
            _profile = new InMemorySaveStorage();
        }

        [Test]
        public void Undo_GoesBackToTheLastCheckpoint_Once()
        {
            var undo = new UndoService(new SaveSerializer(), _profile, _clock);
            using (LifeSession life = SaveFixture.NewLife())
            {
                undo.Checkpoint(life, SaveFixture.Content.ContentVersion);
                var before = life.TimeState.Clock.Now;
                life.Advance(TimeSkip.Month);

                LifeSessionLoadResult restored = undo.Undo(life, SaveFixture.Content, new EventBus());
                using (restored.Session)
                {
                    Assert.That(restored.Session.TimeState.Clock.Now, Is.EqualTo(before));
                    Assert.That(undo.Quote(restored.Session).ReasonKeys, Is.EquivalentTo(new[] { UndoQuote.ReasonNoSnapshot }));
                    Assert.That(undo.Quote(restored.Session).UsesLeftToday, Is.EqualTo(UndoService.DefaultDailyLimit - 1));
                }
            }
        }

        [Test]
        public void DailyLimit_ResetsOnANewDay_AndSurvivesANewService()
        {
            using (LifeSession life = SaveFixture.NewLife())
            {
                for (int i = 0; i < UndoService.DefaultDailyLimit; i++)
                {
                    var undo = new UndoService(new SaveSerializer(), _profile, _clock);
                    undo.Checkpoint(life, SaveFixture.Content.ContentVersion);
                    undo.Undo(life, SaveFixture.Content, new EventBus()).Session.Dispose();
                }
                var next = new UndoService(new SaveSerializer(), _profile, _clock);
                next.Checkpoint(life, SaveFixture.Content.ContentVersion);

                Assert.That(next.Quote(life).ReasonKeys, Is.EquivalentTo(new[] { UndoQuote.ReasonDailyLimit }));
                Assert.Throws<InvalidOperationException>(() => next.Undo(life, SaveFixture.Content, new EventBus()));
                _clock.Today = _clock.Today.AddDays(1);
                Assert.That(next.Quote(life).Available, Is.True);
            }
        }

        [Test]
        public void IronmanLives_CannotUndo()
        {
            var undo = new UndoService(new SaveSerializer(), _profile, _clock);
            using (LifeSession life = SaveFixture.NewLife(ironman: true))
            {
                undo.Checkpoint(life, SaveFixture.Content.ContentVersion);

                Assert.That(undo.HasSnapshot, Is.False);
                Assert.That(undo.Quote(life).ReasonKeys, Is.EquivalentTo(new[] { UndoQuote.ReasonIronman, UndoQuote.ReasonNoSnapshot }));
            }
        }

        [Test]
        public void GamblingAfterTheCheckpoint_CannotBeUndone()
        {
            var undo = new UndoService(new SaveSerializer(), _profile, _clock);
            using (LifeSession life = SaveFixture.NewLife())
            {
                SaveFixture.Advance(life, 20);
                life.Character.Wallet.Deposit(10000m);
                undo.Checkpoint(life, SaveFixture.Content.ContentVersion);
                Assert.That(undo.Quote(life).Available, Is.True);

                life.Vices.Indulge("betting");

                Assert.That(undo.Quote(life).ReasonKeys, Is.EquivalentTo(new[] { UndoQuote.ReasonIrreversible }));
            }
        }

        private sealed class FakeClock : IRealTimeClock
        {
            public DateTime Today { get; set; }
        }
    }
}
