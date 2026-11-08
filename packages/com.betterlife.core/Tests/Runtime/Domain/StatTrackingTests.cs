using System;
using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Domain
{
    /// <summary>Tests for <see cref="StatChangeLog"/>, <see cref="StatHistory"/> and <see cref="ReputationBook"/>.</summary>
    [TestFixture]
    public class StatTrackingTests
    {
        private static readonly GameDate Week10 = new GameDate(10);

        [Test]
        public void ChangeLog_KeepsOnlyRetentionWindow()
        {
            var log = new StatChangeLog(4);
            log.Add(new StatChangeLogEntry(new GameDate(6), StatIds.Stress, 1f, "source.a"));
            log.Add(new StatChangeLogEntry(new GameDate(7), StatIds.Stress, 2f, "source.a"));
            log.Add(new StatChangeLogEntry(Week10, StatIds.Stress, 3f, "source.b"));

            Assert.That(log.Entries.Select(e => e.Week.TotalWeeks), Is.EqualTo(new[] { 7, 10 }));
        }

        [Test]
        public void ChangeLog_SummarizesPerSource_LargestFirst()
        {
            var log = new StatChangeLog();
            log.Add(new StatChangeLogEntry(Week10, StatIds.Stress, 2f, "source.work"));
            log.Add(new StatChangeLogEntry(Week10, StatIds.Stress, 6f, "source.work"));
            log.Add(new StatChangeLogEntry(Week10, StatIds.Stress, -3f, "source.exercise"));
            log.Add(new StatChangeLogEntry(Week10, StatIds.Fitness, 9f, "source.exercise"));

            var lines = log.Summarize(StatIds.Stress, Week10);

            Assert.That(lines.Select(l => l.LabelKey), Is.EqualTo(new[] { "source.work", "source.exercise" }));
            Assert.That(lines.Select(l => l.Contribution), Is.EqualTo(new[] { 8f, -3f }));
        }

        [Test]
        public void ChangeLog_RejectsNonPositiveRetention()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new StatChangeLog(0));
        }

        [Test]
        public void History_KeepsCapacity_AndReplacesSameWeek()
        {
            var block = new StatBlock(TestCatalogs.Stats());
            var history = new StatHistory(new[] { StatIds.Wealth }, 3);

            for (int week = 0; week < 5; week++)
            {
                block.SetStored(StatIds.Wealth, week * 10f);
                history.Record(new GameDate(week), block);
            }
            block.SetStored(StatIds.Wealth, 99f);
            history.Record(new GameDate(4), block);

            Assert.That(history.GetSamples(StatIds.Wealth), Is.EqualTo(new[] { 20f, 30f, 99f }));
            Assert.That(history.LastRecordedWeek, Is.EqualTo(new GameDate(4)));
        }

        [Test]
        public void History_RejectsGoingBackInTime_AndUntrackedStats()
        {
            var block = new StatBlock(TestCatalogs.Stats());
            var history = new StatHistory(new[] { StatIds.Wealth });
            history.Record(Week10, block);

            Assert.Throws<ArgumentOutOfRangeException>(() => history.Record(new GameDate(9), block));
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => history.GetSamples(StatIds.Stress));
        }

        [Test]
        public void Reputation_DefaultsToUnknown_AndClamps()
        {
            var book = new ReputationBook();

            Assert.That(book.Get("business"), Is.EqualTo(ReputationBook.Unknown));
            Assert.That(book.Change("business", 30f), Is.EqualTo(30f));
            Assert.That(book.Change("business", 200f), Is.EqualTo(100f));
            Assert.That(book.Change("underworld", -10f), Is.EqualTo(0f));
            Assert.Throws<ArgumentException>(() => book.Get("Bad Domain"));
            Assert.Throws<ArgumentException>(() => book.Change("business", float.NaN));
        }
    }
}
