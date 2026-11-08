using System;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Time
{
    /// <summary>Tests for <see cref="CalendarDate"/>.</summary>
    [TestFixture]
    public class CalendarDateTests
    {
        [TestCase(2023, 2, 29)]
        [TestCase(1900, 2, 29)]
        [TestCase(2024, 13, 1)]
        [TestCase(2024, 0, 1)]
        [TestCase(2024, 4, 31)]
        [TestCase(2024, 1, 0)]
        [TestCase(0, 1, 1)]
        public void Constructor_RejectsInvalidDates(int year, int month, int day)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CalendarDate(year, month, day));
        }

        [TestCase(2024, 2, 29)]
        [TestCase(2000, 2, 29)]
        public void Constructor_AcceptsLeapDays(int year, int month, int day)
        {
            Assert.DoesNotThrow(() => new CalendarDate(year, month, day));
        }

        [Test]
        public void DayOfWeekAndDayOfYear_MatchSystemDateTime_ForManyDates()
        {
            var cursor = new DateTime(1920, 1, 1);
            var end = new DateTime(2040, 12, 31);
            while (cursor <= end)
            {
                var date = new CalendarDate(cursor.Year, cursor.Month, cursor.Day);
                Assert.That(date.DayOfWeek, Is.EqualTo(cursor.DayOfWeek), date.ToString());
                Assert.That(date.DayOfYear, Is.EqualTo(cursor.DayOfYear - 1), date.ToString());
                cursor = cursor.AddDays(3);
            }
        }

        [Test]
        public void ToGameDate_MapsFirstAndLastDaysOfYear()
        {
            Assert.That(new CalendarDate(2026, 1, 1).ToGameDate(), Is.EqualTo(GameDate.FromYearWeek(2026, 0)));
            Assert.That(new CalendarDate(2026, 1, 8).ToGameDate(), Is.EqualTo(GameDate.FromYearWeek(2026, 1)));
            Assert.That(new CalendarDate(2026, 12, 31).ToGameDate(), Is.EqualTo(GameDate.FromYearWeek(2026, 51)));
            Assert.That(new CalendarDate(2024, 12, 31).ToGameDate(), Is.EqualTo(GameDate.FromYearWeek(2024, 51)));
        }

        [Test]
        public void ToGameDate_KeepsMonthForMidMonthDates()
        {
            for (int month = 1; month <= 12; month++)
            {
                Assert.That(new CalendarDate(2026, month, 15).ToGameDate().Month, Is.EqualTo(month), $"Month {month}");
            }
        }

        [Test]
        public void AgeAt_IncrementsOnBirthdayWeek()
        {
            var birth = new CalendarDate(2000, 6, 15);
            GameDate birthWeek = birth.ToGameDate();

            Assert.That(birth.AgeAt(birthWeek), Is.EqualTo(0));
            Assert.That(birth.AgeAt(GameDate.FromYearWeek(2020, birthWeek.WeekOfYear).AddWeeks(-1)), Is.EqualTo(19));
            Assert.That(birth.AgeAt(GameDate.FromYearWeek(2020, birthWeek.WeekOfYear)), Is.EqualTo(20));
        }

        [Test]
        public void AgeAt_ThrowsBeforeBirth()
        {
            var birth = new CalendarDate(2000, 6, 15);

            Assert.Throws<ArgumentOutOfRangeException>(() => birth.AgeAt(birth.ToGameDate().AddWeeks(-1)));
        }

        [Test]
        public void Equality_IsByValue()
        {
            Assert.That(new CalendarDate(2000, 1, 2) == new CalendarDate(2000, 1, 2), Is.True);
            Assert.That(new CalendarDate(2000, 1, 2) != new CalendarDate(2000, 2, 1), Is.True);
        }
    }
}
