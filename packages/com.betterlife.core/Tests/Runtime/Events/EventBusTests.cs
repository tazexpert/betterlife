using System;
using System.Collections.Generic;
using BetterLife.Core.Events;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Events
{
    /// <summary>Tests for <see cref="EventBus"/>.</summary>
    [TestFixture]
    public class EventBusTests
    {
        private sealed class JobLost : IGameEvent
        {
        }

        private sealed class StressRaised : IGameEvent
        {
        }

        private EventBus _bus;
        private List<string> _log;

        [SetUp]
        public void SetUp()
        {
            _bus = new EventBus();
            _log = new List<string>();
        }

        [Test]
        public void Publish_DeliversToSubscribersInOrder()
        {
            _bus.Subscribe<JobLost>(_ => _log.Add("a"));
            _bus.Subscribe<JobLost>(_ => _log.Add("b"));

            _bus.Publish(new JobLost());

            Assert.That(_log, Is.EqualTo(new[] { "a", "b" }));
        }

        [Test]
        public void Publish_OnlyReachesMatchingType()
        {
            _bus.Subscribe<StressRaised>(_ => _log.Add("stress"));

            _bus.Publish(new JobLost());

            Assert.That(_log, Is.Empty);
        }

        [Test]
        public void Dispose_Unsubscribes_AndIsIdempotent()
        {
            IDisposable token = _bus.Subscribe<JobLost>(_ => _log.Add("a"));

            token.Dispose();
            token.Dispose();
            _bus.Publish(new JobLost());

            Assert.That(_log, Is.Empty);
        }

        [Test]
        public void NestedPublish_IsQueuedUntilCurrentEventReachesEveryHandler()
        {
            _bus.Subscribe<JobLost>(_ =>
            {
                _log.Add("job:1");
                _bus.Publish(new StressRaised());
            });
            _bus.Subscribe<JobLost>(_ => _log.Add("job:2"));
            _bus.Subscribe<StressRaised>(_ => _log.Add("stress"));

            _bus.Publish(new JobLost());

            Assert.That(_log, Is.EqualTo(new[] { "job:1", "job:2", "stress" }));
        }

        [Test]
        public void ThrowingHandler_DoesNotStopOthers_AndErrorsAreAggregated()
        {
            _bus.Subscribe<JobLost>(_ => throw new InvalidOperationException("boom"));
            _bus.Subscribe<JobLost>(_ => _log.Add("after"));

            var ex = Assert.Throws<AggregateException>(() => _bus.Publish(new JobLost()));

            Assert.That(_log, Is.EqualTo(new[] { "after" }));
            Assert.That(ex.InnerExceptions, Has.Count.EqualTo(1));
            Assert.That(ex.InnerExceptions[0], Is.TypeOf<InvalidOperationException>());
        }

        [Test]
        public void BusRecovers_AfterHandlerThrows()
        {
            IDisposable bad = _bus.Subscribe<JobLost>(_ => throw new InvalidOperationException());
            Assert.Throws<AggregateException>(() => _bus.Publish(new JobLost()));
            bad.Dispose();
            _bus.Subscribe<JobLost>(_ => _log.Add("ok"));

            _bus.Publish(new JobLost());

            Assert.That(_log, Is.EqualTo(new[] { "ok" }));
        }

        [Test]
        public void UnsubscribeDuringDispatch_StillCompletesCurrentDispatch()
        {
            IDisposable second = null;
            _bus.Subscribe<JobLost>(_ =>
            {
                _log.Add("first");
                second.Dispose();
            });
            second = _bus.Subscribe<JobLost>(_ => _log.Add("second"));

            _bus.Publish(new JobLost());
            _bus.Publish(new JobLost());

            Assert.That(_log, Is.EqualTo(new[] { "first", "second", "first" }));
        }

        [Test]
        public void Guards_RejectNulls()
        {
            Assert.Throws<ArgumentNullException>(() => _bus.Subscribe<JobLost>(null));
            Assert.Throws<ArgumentNullException>(() => _bus.Publish<JobLost>(null));
        }
    }
}
