using System;
using System.Collections.Generic;
using BetterLife.Core.Randomness;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Randomness
{
    /// <summary>Tests for <see cref="RandomRegistry"/>.</summary>
    [TestFixture]
    public class RandomRegistryTests
    {
        [Test]
        public void GetStream_ReturnsSameInstanceForSameName()
        {
            var registry = new RandomRegistry(1UL);

            Assert.That(registry.GetStream("events"), Is.SameAs(registry.GetStream("events")));
        }

        [Test]
        public void Streams_AreIndependentOfEachOther()
        {
            var a = new RandomRegistry(1UL);
            var b = new RandomRegistry(1UL);

            // Consuming "events" in one life must not change "npc".
            for (int i = 0; i < 10; i++)
            {
                a.GetStream("events").NextUInt();
            }

            Assert.That(a.GetStream("npc").NextUInt(), Is.EqualTo(b.GetStream("npc").NextUInt()));
        }

        [Test]
        public void DifferentLifeSeeds_GiveDifferentStreams()
        {
            Assert.That(new RandomRegistry(1UL).GetStream("events").NextUInt(),
                Is.Not.EqualTo(new RandomRegistry(2UL).GetStream("events").NextUInt()));
        }

        [Test]
        public void RestoreAll_UpdatesExistingInstancesInPlace()
        {
            var registry = new RandomRegistry(5UL);
            IRandomProvider events = registry.GetStream("events");
            IReadOnlyDictionary<string, RandomState> snapshot = registry.CaptureAll();
            uint expected = events.NextUInt();

            registry.RestoreAll(snapshot);

            Assert.That(events.NextUInt(), Is.EqualTo(expected));
        }

        [Test]
        public void GetStream_RejectsEmptyName()
        {
            Assert.Throws<ArgumentException>(() => new RandomRegistry(1UL).GetStream(""));
        }

        [Test]
        public void RestoreAll_RejectsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new RandomRegistry(1UL).RestoreAll(null));
        }
    }
}
