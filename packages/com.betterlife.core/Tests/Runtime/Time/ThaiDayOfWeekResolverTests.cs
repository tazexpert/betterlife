using System;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Time
{
    /// <summary>Tests for <see cref="ThaiDayOfWeekResolver"/>.</summary>
    [TestFixture]
    public class ThaiDayOfWeekResolverTests
    {
        // 2026-11-11 is a Wednesday, 2026-11-12 a Thursday, 2026-11-08 a Sunday.
        [TestCase(2026, 11, 11, 10, ThaiDayOfWeek.Wednesday)]
        [TestCase(2026, 11, 11, 6, ThaiDayOfWeek.Wednesday)]
        [TestCase(2026, 11, 11, 17, ThaiDayOfWeek.Wednesday)]
        [TestCase(2026, 11, 11, 18, ThaiDayOfWeek.WednesdayNight)]
        [TestCase(2026, 11, 11, 23, ThaiDayOfWeek.WednesdayNight)]
        [TestCase(2026, 11, 12, 0, ThaiDayOfWeek.WednesdayNight)]
        [TestCase(2026, 11, 12, 5, ThaiDayOfWeek.WednesdayNight)]
        [TestCase(2026, 11, 12, 6, ThaiDayOfWeek.Thursday)]
        [TestCase(2026, 11, 11, 3, ThaiDayOfWeek.Tuesday)]
        [TestCase(2026, 11, 8, 3, ThaiDayOfWeek.Saturday)]
        [TestCase(2026, 11, 8, 20, ThaiDayOfWeek.Sunday)]
        public void Resolve_AppliesSunriseAndRahuRules(int year, int month, int day, int hour, ThaiDayOfWeek expected)
        {
            Assert.That(ThaiDayOfWeekResolver.Resolve(new CalendarDate(year, month, day), hour), Is.EqualTo(expected));
        }

        [TestCase(-1)]
        [TestCase(24)]
        public void Resolve_RejectsInvalidHour(int hour)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => ThaiDayOfWeekResolver.Resolve(new CalendarDate(2026, 1, 1), hour));
        }
    }
}
