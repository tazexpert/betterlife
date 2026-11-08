using System;
using System.Collections.Generic;
using BetterLife.Core.Randomness;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Randomness
{
    /// <summary>Tests for <see cref="Pcg32"/>.</summary>
    [TestFixture]
    public class Pcg32Tests
    {
        private const int SampleCount = 10000;

        [Test]
        public void MatchesReferenceImplementationOutput()
        {
            // Expected values from the official pcg32-demo (seed 42, sequence 54).
            var rng = new Pcg32(42UL, 54UL);
            uint[] expected = { 0xa15c02b7, 0x7b47f409, 0xba1d3330, 0x83d2f293, 0xbfa4784b, 0xcbed606e };

            foreach (uint value in expected)
            {
                Assert.That(rng.NextUInt(), Is.EqualTo(value));
            }
        }

        [Test]
        public void SameSeed_ProducesSameSequence_DifferentSeedDiffers()
        {
            var a = new Pcg32(7UL, 1UL);
            var b = new Pcg32(7UL, 1UL);
            var c = new Pcg32(8UL, 1UL);
            bool anyDifferent = false;

            for (int i = 0; i < 100; i++)
            {
                uint va = a.NextUInt();
                Assert.That(b.NextUInt(), Is.EqualTo(va));
                anyDifferent |= c.NextUInt() != va;
            }
            Assert.That(anyDifferent, Is.True);
        }

        [TestCase(0, 1)]
        [TestCase(-5, 5)]
        [TestCase(1, 7)]
        [TestCase(int.MinValue, int.MaxValue)]
        public void NextInt_StaysInRange(int min, int max)
        {
            var rng = new Pcg32(1UL, 2UL);
            for (int i = 0; i < SampleCount; i++)
            {
                int value = rng.NextInt(min, max);
                Assert.That(value, Is.GreaterThanOrEqualTo(min).And.LessThan(max));
            }
        }

        [Test]
        public void NextInt_CoversEveryValueOfSmallRange()
        {
            var rng = new Pcg32(3UL, 4UL);
            var seen = new HashSet<int>();
            for (int i = 0; i < SampleCount; i++)
            {
                seen.Add(rng.NextInt(1, 7));
            }
            Assert.That(seen, Is.EquivalentTo(new[] { 1, 2, 3, 4, 5, 6 }));
        }

        [TestCase(5, 5)]
        [TestCase(5, 4)]
        public void NextInt_RejectsEmptyRange(int min, int max)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new Pcg32(1UL, 1UL).NextInt(min, max));
        }

        [Test]
        public void NextFloat_IsInUnitInterval_WithReasonableMean()
        {
            var rng = new Pcg32(9UL, 9UL);
            double sum = 0;
            for (int i = 0; i < SampleCount; i++)
            {
                float value = rng.NextFloat();
                Assert.That(value, Is.GreaterThanOrEqualTo(0f).And.LessThan(1f));
                sum += value;
            }
            Assert.That(sum / SampleCount, Is.EqualTo(0.5).Within(0.02));
        }

        [Test]
        public void Chance_HandlesBoundsAndClamping()
        {
            var rng = new Pcg32(5UL, 5UL);
            for (int i = 0; i < 1000; i++)
            {
                Assert.That(rng.Chance(0f), Is.False);
                Assert.That(rng.Chance(-1f), Is.False);
                Assert.That(rng.Chance(1f), Is.True);
                Assert.That(rng.Chance(2f), Is.True);
            }
        }

        [Test]
        public void Chance_AlwaysConsumesOneValue()
        {
            var a = new Pcg32(5UL, 5UL);
            var b = new Pcg32(5UL, 5UL);

            a.Chance(0f);
            b.Chance(0.7f);

            Assert.That(a.NextUInt(), Is.EqualTo(b.NextUInt()));
        }

        [Test]
        public void Chance_RejectsNaN()
        {
            Assert.Throws<ArgumentException>(() => new Pcg32(1UL, 1UL).Chance(float.NaN));
        }

        [Test]
        public void CaptureAndRestore_ReplaysSequence()
        {
            var rng = new Pcg32(11UL, 12UL);
            rng.NextUInt();
            RandomState snapshot = rng.CaptureState();
            uint first = rng.NextUInt();
            uint second = rng.NextUInt();

            rng.RestoreState(snapshot);

            Assert.That(rng.NextUInt(), Is.EqualTo(first));
            Assert.That(rng.NextUInt(), Is.EqualTo(second));
        }

        [Test]
        public void RestoreState_RejectsEvenIncrement()
        {
            Assert.Throws<ArgumentException>(() => new Pcg32(1UL, 1UL).RestoreState(new RandomState(1UL, 1UL, 2UL)));
        }

        [Test]
        public void Fork_IsDeterministic_AndIndependentOfConsumption()
        {
            var a = new Pcg32(100UL, 1UL);
            var b = new Pcg32(100UL, 1UL);
            for (int i = 0; i < 50; i++)
            {
                b.NextUInt();
            }

            Assert.That(a.Fork("npc").NextUInt(), Is.EqualTo(b.Fork("npc").NextUInt()));
            Assert.That(a.Fork("npc").NextUInt(), Is.Not.EqualTo(a.Fork("events").NextUInt()));
        }

        [Test]
        public void Fork_SurvivesRestore()
        {
            var original = new Pcg32(100UL, 1UL);
            var restored = new Pcg32(1UL, 1UL);
            restored.RestoreState(original.CaptureState());

            Assert.That(restored.Fork("x").NextUInt(), Is.EqualTo(original.Fork("x").NextUInt()));
        }

        [TestCase(null)]
        [TestCase("")]
        public void Fork_RejectsEmptyName(string name)
        {
            Assert.Throws<ArgumentException>(() => new Pcg32(1UL, 1UL).Fork(name));
        }
    }
}
