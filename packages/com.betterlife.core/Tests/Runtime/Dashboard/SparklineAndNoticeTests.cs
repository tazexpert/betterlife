using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Notices;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.ViewModels;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Dashboard
{
    /// <summary>Tests for <see cref="Sparkline"/> and <see cref="NoticeFeed"/>.</summary>
    [TestFixture]
    public class SparklineAndNoticeTests
    {
        private static readonly GameDate Week = GameDate.FromYearWeek(2030, 10);

        [Test]
        public void Points_FitTheData_InAUnitBox()
        {
            IReadOnlyList<SparkPoint> points = Sparkline.Points(new[] { 20f, 40f, 30f });

            Assert.That(points.Select(p => p.X), Is.EqualTo(new[] { 0f, 0.5f, 1f }));
            Assert.That(points.Select(p => p.Y), Is.EqualTo(new[] { 0f, 1f, 0.5f }));
        }

        [Test]
        public void Points_UseAMinimumSpan_AndNeedTwoSamples()
        {
            IReadOnlyList<SparkPoint> flat = Sparkline.Points(new[] { 50f, 50f, 52f });
            Assert.That(flat[0].Y, Is.EqualTo(0.4f).Within(1e-5f), "a 2-point wobble is drawn inside a 10-point span, centred");
            Assert.That(flat[2].Y, Is.EqualTo(0.6f).Within(1e-5f));
            Assert.That(Sparkline.Points(new[] { 50f }), Is.Empty);
            Assert.That(Sparkline.Points(null), Is.Empty);
        }

        [Test]
        public void Trend_ComparesWithFourWeeksAgo()
        {
            var samples = new[] { 10f, 50f, 60f, 60f, 60f, 62f };
            Assert.That(Sparkline.Delta(samples), Is.EqualTo(12f), "62 - 50");
            Assert.That(Sparkline.Delta(new[] { 40f, 45f }), Is.EqualTo(5f), "short history uses the oldest sample");
            Assert.That(Sparkline.Delta(new[] { 40f }), Is.EqualTo(0f));
            Assert.That(Sparkline.Direction(0.4f), Is.EqualTo(TrendDirection.Flat));
            Assert.That(Sparkline.Direction(2f), Is.EqualTo(TrendDirection.Up));
            Assert.That(Sparkline.Direction(-2f), Is.EqualTo(TrendDirection.Down));
        }

        [Test]
        public void Feed_KeepsNewestFirst_UpToCapacity()
        {
            var feed = new NoticeFeed(3);
            feed.Add(Result(Interrupt("a", 0), Interrupt("b", 1)), null);
            feed.Add(Result(Interrupt("c", 2), Interrupt("d", 3)), null);

            Assert.That(feed.Items.Select(n => n.SourceId), Is.EqualTo(new[] { "d", "c", "b" }));
            Assert.That(feed.Items[0].TitleKey, Is.EqualTo("interrupt.d"));
            feed.Clear();
            Assert.That(feed.Items, Is.Empty);
        }

        [Test]
        public void Feed_SkipsPendingDecisions_AndFindsTheOneThatStoppedTime()
        {
            InterruptRequest offer = Interrupt("test.offer", 2, Severity.Major);
            var inbox = new[] { new PendingEvent(7, "test.offer", EventOrigin.Random, Week.AddWeeks(2), Week.AddWeeks(6)) };
            var result = new AdvanceResult(Week, Week.AddWeeks(2), 52, new[] { Interrupt("job", 1), offer }, offer);
            var feed = new NoticeFeed();

            feed.Add(result, inbox);

            Assert.That(feed.Items.Select(n => n.SourceId), Is.EqualTo(new[] { "job" }));
            Assert.That(NoticeFeed.StoppingDecision(result, inbox), Is.EqualTo(7));
            Assert.That(NoticeFeed.StoppingDecision(Result(Interrupt("job", 1)), inbox), Is.Null, "not stopped");
        }

        private static InterruptRequest Interrupt(string source, int week, Severity severity = Severity.Normal)
        {
            return new InterruptRequest(severity, "interrupt." + source, source, Week.AddWeeks(week));
        }

        private static AdvanceResult Result(params InterruptRequest[] interrupts)
        {
            return new AdvanceResult(Week, Week.AddWeeks(4), 4, interrupts, null);
        }
    }
}
