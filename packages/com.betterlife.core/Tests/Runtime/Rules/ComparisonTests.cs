using System;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Rules
{
    /// <summary>Tests for <see cref="Comparison"/>, <see cref="FloatRange"/> and <see cref="DecimalRange"/>.</summary>
    [TestFixture]
    public class ComparisonTests
    {
        [TestCase(5f, ComparisonOperator.Equal, 5f, true)]
        [TestCase(5f, ComparisonOperator.Equal, 5.00001f, true)]
        [TestCase(5f, ComparisonOperator.NotEqual, 6f, true)]
        [TestCase(5f, ComparisonOperator.Less, 5f, false)]
        [TestCase(5f, ComparisonOperator.LessOrEqual, 5f, true)]
        [TestCase(6f, ComparisonOperator.Greater, 5f, true)]
        [TestCase(70f, ComparisonOperator.GreaterOrEqual, 69.99999f, true)]
        [TestCase(4f, ComparisonOperator.GreaterOrEqual, 5f, false)]
        public void Compare_Floats(float left, ComparisonOperator op, float right, bool expected)
        {
            Assert.That(Comparison.Compare(left, op, right), Is.EqualTo(expected));
        }

        [Test]
        public void Compare_DecimalsExactly()
        {
            Assert.That(Comparison.Compare(100.01m, ComparisonOperator.Greater, 100m), Is.True);
            Assert.That(Comparison.Compare(25, ComparisonOperator.Less, 25), Is.False);
        }

        [Test]
        public void Symbols_RoundTrip_AndUnknownIsRejected()
        {
            foreach (ComparisonOperator op in Enum.GetValues(typeof(ComparisonOperator)))
            {
                Assert.That(Comparison.TryParseSymbol(Comparison.ToSymbol(op), out ComparisonOperator parsed), Is.True);
                Assert.That(parsed, Is.EqualTo(op));
            }
            Assert.That(Comparison.TryParseSymbol("=>", out _), Is.False);
        }

        [Test]
        public void FixedRange_ConsumesNoRandomness()
        {
            var a = new Pcg32(1UL, 1UL);
            var b = new Pcg32(1UL, 1UL);

            Assert.That(FloatRange.Fixed(5f).Roll(a), Is.EqualTo(5f));
            Assert.That(DecimalRange.Fixed(5m).Roll(a), Is.EqualTo(5m));
            Assert.That(a.NextUInt(), Is.EqualTo(b.NextUInt()));
        }

        [Test]
        public void Ranges_RollWithinBounds_AndMoneyRoundsToSatang()
        {
            var random = new Pcg32(3UL, 3UL);
            var floats = new FloatRange(3f, 8f);
            var money = new DecimalRange(100m, 200m);

            for (int i = 0; i < 1000; i++)
            {
                Assert.That(floats.Roll(random), Is.InRange(3f, 8f));
                decimal amount = money.Roll(random);
                Assert.That(amount, Is.InRange(100m, 200m));
                Assert.That(decimal.Round(amount, 2), Is.EqualTo(amount));
            }
        }

        [Test]
        public void Ranges_RejectInvertedOrNonFiniteBounds()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new FloatRange(5f, 4f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FloatRange(float.NaN, 4f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new DecimalRange(5m, 4m));
        }
    }
}
