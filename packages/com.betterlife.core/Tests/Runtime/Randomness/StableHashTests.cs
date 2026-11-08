using System;
using BetterLife.Core.Randomness;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Randomness
{
    /// <summary>Tests for <see cref="StableHash"/>.</summary>
    [TestFixture]
    public class StableHashTests
    {
        [Test]
        public void Fnv1a64_MatchesKnownVectors()
        {
            // Published FNV-1a 64-bit test vectors.
            Assert.That(StableHash.Fnv1a64("a"), Is.EqualTo(0xaf63dc4c8601ec8cUL));
            Assert.That(StableHash.Fnv1a64("foobar"), Is.EqualTo(0x85944171f73967e8UL));
        }

        [Test]
        public void Fnv1a64_HandlesThaiText()
        {
            Assert.That(StableHash.Fnv1a64("ดวง"), Is.EqualTo(StableHash.Fnv1a64("ดวง")));
            Assert.That(StableHash.Fnv1a64("ดวง"), Is.Not.EqualTo(StableHash.Fnv1a64("กรรม")));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Fnv1a64_RejectsEmpty(string value)
        {
            Assert.Throws<ArgumentException>(() => StableHash.Fnv1a64(value));
        }
    }
}
