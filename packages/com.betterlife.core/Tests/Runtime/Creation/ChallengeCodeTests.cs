using BetterLife.Core.Creation;
using BetterLife.Core.Randomness;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Creation
{
    /// <summary>Tests for <see cref="ChallengeCode"/>.</summary>
    [TestFixture]
    public class ChallengeCodeTests
    {
        [TestCase(0UL)]
        [TestCase(1UL)]
        [TestCase(ulong.MaxValue)]
        [TestCase(0xdeadbeefcafef00dUL)]
        public void RoundTrips(ulong seed)
        {
            string code = ChallengeCode.Encode(seed);

            Assert.That(code, Does.Match("^[0-9A-HJKMNP-TV-Z]{4}-[0-9A-HJKMNP-TV-Z]{4}-[0-9A-HJKMNP-TV-Z]{5}$"));
            Assert.That(ChallengeCode.TryDecode(code, out ulong decoded), Is.True);
            Assert.That(decoded, Is.EqualTo(seed));
        }

        [Test]
        public void ManyRandomSeeds_RoundTrip()
        {
            var random = new Pcg32(3UL, 3UL);
            for (int i = 0; i < 1000; i++)
            {
                ulong seed = ((ulong)random.NextUInt() << 32) | random.NextUInt();
                Assert.That(ChallengeCode.TryDecode(ChallengeCode.Encode(seed), out ulong decoded) && decoded == seed, Is.True);
            }
        }

        [Test]
        public void Decode_IsForgivingAboutTyping()
        {
            string code = ChallengeCode.Encode(123456789UL);
            string sloppy = " " + code.ToLowerInvariant().Replace("-", " ").Replace('1', 'l').Replace('0', 'o') + " ";

            Assert.That(ChallengeCode.TryDecode(sloppy, out ulong decoded), Is.True);
            Assert.That(decoded, Is.EqualTo(123456789UL));
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("ABCD-EFGH-JKM")]
        [TestCase("ABCD-EFGH-JKMNPQ")]
        [TestCase("ABCD-EFGH-JKMNU")]
        [TestCase("ZZZZ-ZZZZ-ZZZZZ")]
        public void Decode_RejectsInvalidCodes(string code)
        {
            Assert.That(ChallengeCode.TryDecode(code, out _), Is.False);
        }
    }
}
