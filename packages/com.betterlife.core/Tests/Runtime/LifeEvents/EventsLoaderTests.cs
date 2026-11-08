using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Tests.Content;
using NUnit.Framework;

namespace BetterLife.Core.Tests.LifeEvents
{
    /// <summary>Tests for the events loader's validation.</summary>
    [TestFixture]
    public class EventsLoaderTests
    {
        private static readonly string Choice = "{ 'id': 'go', 'textKey': 'c.go', 'success': { 'resultKey': 'r.go' } }";

        [TestCase(", 'choices': [ { 'id': 'go', 'textKey': 'c.go', 'chance': { 'base': 50 }, 'success': { 'resultKey': 'r.go' } } ], 'defaultChoice': 'go'",
            "$.events[0].choices[0].failure", "required when the choice has a chance")]
        [TestCase(", 'choices': [ { 'id': 'go', 'textKey': 'c.go', 'success': { 'resultKey': 'r.go' }, 'failure': { 'resultKey': 'r.no' } } ], 'defaultChoice': 'go'",
            "$.events[0].choices[0].failure", "only allowed when the choice has a chance")]
        [TestCase(", 'choices': [ CHOICE ], 'defaultChoice': 'stay'", "$.events[0]", "Default choice 'stay'")]
        [TestCase(", 'choices': [ CHOICE ]", "$.events[0]", "Default choice ''")]
        [TestCase(", 'choices': [ CHOICE, CHOICE ], 'defaultChoice': 'go'", "$.events[0]", "Choice ids must be unique")]
        [TestCase(", 'debtAmount': { 'min': 0, 'max': 5 }, 'choices': []", "$.events[0]", "Only karmic debt events")]
        [TestCase(", 'onFire': [ { 'type': 'event.schedule', 'event': 'test.missing', 'delayWeeks': 2 } ], 'choices': []",
            "$.events[0].onFire[0].event", "unknown event 'test.missing'")]
        [TestCase(", 'onFire': [ { 'type': 'event.schedule', 'event': 'test.other', 'delayWeeks': 0 } ], 'choices': []", "$.events[0].onFire[0]", "1 <= min <= max")]
        [TestCase(", 'weight': { 'base': -1 }, 'choices': []", "$.events[0]", "Base weight must be non-negative")]
        [TestCase(", 'severity': 'apocalyptic', 'choices': []", "$.events[0].severity", "not one of")]
        public void InvalidEvents_ReportFileAndPath(string extra, string path, string message)
        {
            string ev = "{ 'id': 'test.a', 'titleKey': 'event.a.title', 'bodyKey': 'event.a.body', 'category': 'test', 'trigger': 'random', "
                + "'tone': 'neutral'" + (extra.Contains("'severity'") ? string.Empty : ", 'severity': 'minor'") + extra.Replace("CHOICE", Choice) + " }";
            InMemoryContentSource source = TestContent.Source("{ 'type': 'events', 'path': 'events/a.json' }, { 'type': 'events', 'path': 'events/b.json' }")
                .Add("events/a.json", "{ 'events': [ " + ev + " ] }")
                .Add("events/b.json", "{ 'events': [ " + EventFixture.Simple("test.other", trigger: "scheduled") + " ] }");

            ContentError error = DefaultContent.CreatePipeline(source).Load().Errors.First();

            Assert.That(error.File, Is.EqualTo("events/a.json"));
            Assert.That(error.Path, Is.EqualTo(path));
            Assert.That(error.Message, Does.Contain(message));
        }

        [Test]
        public void ScheduleTargets_MayLiveInAnotherFile()
        {
            InMemoryContentSource source = TestContent.Source("{ 'type': 'events', 'path': 'events/a.json' }, { 'type': 'events', 'path': 'events/b.json' }")
                .Add("events/a.json", "{ 'events': [ " + EventFixture.Simple("test.a", ", 'onFire': [ { 'type': 'event.schedule', 'event': 'test.b', 'delayWeeks': { 'min': 2, 'max': 4 } } ]") + " ] }")
                .Add("events/b.json", "{ 'events': [ " + EventFixture.Simple("test.b", trigger: "scheduled") + " ] }");

            Assert.That(DefaultContent.CreatePipeline(source).Load().Errors, Is.Empty);
        }
    }
}
