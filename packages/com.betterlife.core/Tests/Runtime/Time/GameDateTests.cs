using System;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Time
{
    /// <summary>Tests for <see cref="GameDate"/>.</summary>
    [TestFixture]
    public class GameDateTests
    {
        [Test]
        public void Epoch_IsFirstWeekOfJanuary1926()
        {
            var date = new GameDate(0);

            Assert.That(date.Year, Is.EqualTo(1926));
            Assert.That(date.WeekOfYear, Is.EqualTo(0));
            Assert.That(date.Month, Is.EqualTo(1));
            Assert.That(date.IsYearStart, Is.True);
            Assert.That(date.IsMonthStart, Is.True);
        }

        [Test]
        public void FromYearWeek_RoundTrips()
        {
            GameDate date = GameDate.FromYearWeek(2026, 45);

            Assert.That(date.Year, Is.EqualTo(2026));
            Assert.That(date.WeekOfYear, Is.EqualTo(45));
            Assert.That(date.TotalWeeks, Is.EqualTo(100 * 52 + 45));
        }

        [Test]
        public void NegativeWeeks_AreBeforeEpoch()
        {
            var date = new GameDate(-1);

            Assert.That(date.Year, Is.EqualTo(1925));
            Assert.That(date.WeekOfYear, Is.EqualTo(51));
            Assert.That(date.Month, Is.EqualTo(12));
        }

        [Test]
        public void NegativeWholeYear_StartsYear()
        {
            var date = new GameDate(-52);

            Assert.That(date.Year, Is.EqualTo(1925));
            Assert.That(date.WeekOfYear, Is.EqualTo(0));
        }

        [Test]
        public void EveryYear_HasTwelveMonthStarts_AndEachMonthHasFourOrFiveWeeks()
        {
            int[] weeksPerMonth = new int[13];
            int monthStarts = 0;
            for (int week = 0; week < GameCalendar.WeeksPerYear; week++)
            {
                GameDate date = GameDate.FromYearWeek(2000, week);
                weeksPerMonth[date.Month]++;
                if (date.IsMonthStart)
                {
                    monthStarts++;
                }
            }

            Assert.That(monthStarts, Is.EqualTo(12));
            for (int month = 1; month <= 12; month++)
            {
                Assert.That(weeksPerMonth[month], Is.InRange(4, 5), $"Month {month}");
            }
        }

        [Test]
        public void LastWeek_IsDecember_AndNextWeekIsNewYear()
        {
            GameDate last = GameDate.FromYearWeek(1999, 51);
            GameDate next = last.AddWeeks(1);

            Assert.That(last.Month, Is.EqualTo(12));
            Assert.That(next.Year, Is.EqualTo(2000));
            Assert.That(next.IsYearStart, Is.True);
        }

        [Test]
        public void ComparisonAndEquality_FollowTotalWeeks()
        {
            var a = new GameDate(10);
            var b = new GameDate(20);

            Assert.That(a < b, Is.True);
            Assert.That(b >= a, Is.True);
            Assert.That(a == new GameDate(10), Is.True);
            Assert.That(a != b, Is.True);
            Assert.That(a.WeeksUntil(b), Is.EqualTo(10));
            Assert.That(b.WeeksUntil(a), Is.EqualTo(-10));
            Assert.That(a.CompareTo(b), Is.LessThan(0));
        }

        [TestCase(-1)]
        [TestCase(52)]
        public void FromYearWeek_RejectsWeekOutOfRange(int week)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => GameDate.FromYearWeek(2000, week));
        }

        [Test]
        public void AddWeeks_ThrowsOnOverflow()
        {
            Assert.Throws<OverflowException>(() => new GameDate(int.MaxValue).AddWeeks(1));
        }
    }
}
