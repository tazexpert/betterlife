using System;
using System.Collections.Generic;
using BetterLife.Core.Persistence;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Persistence
{
    /// <summary>Tests for <see cref="FoundationDtoMapper"/>.</summary>
    [TestFixture]
    public class FoundationDtoMapperTests
    {
        [Test]
        public void GameDate_RoundTrips()
        {
            GameDate date = GameDate.FromYearWeek(2026, 45);

            Assert.That(FoundationDtoMapper.ToGameDate(FoundationDtoMapper.ToDto(date)), Is.EqualTo(date));
        }

        [Test]
        public void CalendarDate_RoundTrips()
        {
            var date = new CalendarDate(1990, 2, 28);

            Assert.That(FoundationDtoMapper.ToCalendarDate(FoundationDtoMapper.ToDto(date)), Is.EqualTo(date));
        }

        [Test]
        public void CalendarDate_InvalidDtoIsRejected()
        {
            var dto = new CalendarDateDto { Year = 2023, Month = 2, Day = 29 };

            Assert.Throws<ArgumentOutOfRangeException>(() => FoundationDtoMapper.ToCalendarDate(dto));
        }

        [Test]
        public void RandomRegistry_RoundTrip_ContinuesSameSequences()
        {
            var original = new RandomRegistry(0xdeadbeefcafef00dUL);
            original.GetStream("events").NextUInt();
            original.GetStream("npc").NextUInt();

            RandomRegistryDto dto = FoundationDtoMapper.ToDto(original);
            RandomRegistry restored = FoundationDtoMapper.ToRandomRegistry(dto);

            Assert.That(dto.LifeSeed, Is.EqualTo("deadbeefcafef00d"));
            Assert.That(dto.Streams[0].Name, Is.EqualTo("events"));
            Assert.That(restored.LifeSeed, Is.EqualTo(original.LifeSeed));
            foreach (string name in new[] { "events", "npc", "brand-new" })
            {
                Assert.That(restored.GetStream(name).NextUInt(), Is.EqualTo(original.GetStream(name).NextUInt()), name);
            }
        }

        [TestCase("not-hex")]
        [TestCase("")]
        [TestCase(null)]
        [TestCase("1ffffffffffffffff")]
        public void RandomRegistry_RejectsInvalidHex(string lifeSeed)
        {
            var dto = new RandomRegistryDto { LifeSeed = lifeSeed };

            Assert.Throws<FormatException>(() => FoundationDtoMapper.ToRandomRegistry(dto));
        }

        [Test]
        public void RandomRegistry_RejectsStreamWithoutName()
        {
            var dto = new RandomRegistryDto
            {
                LifeSeed = "01",
                Streams = new List<RandomStreamDto> { new RandomStreamDto { Name = "", Seed = "1", State = "1", Increment = "1" } },
            };

            Assert.Throws<FormatException>(() => FoundationDtoMapper.ToRandomRegistry(dto));
        }

        [Test]
        public void NullDtos_AreRejected()
        {
            Assert.Throws<ArgumentNullException>(() => FoundationDtoMapper.ToGameDate(null));
            Assert.Throws<ArgumentNullException>(() => FoundationDtoMapper.ToCalendarDate(null));
            Assert.Throws<ArgumentNullException>(() => FoundationDtoMapper.ToRandomRegistry(null));
            Assert.Throws<ArgumentNullException>(() => FoundationDtoMapper.ToDto((RandomRegistry)null));
        }
    }
}
