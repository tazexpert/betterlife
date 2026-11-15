using System;
using System.Collections.Generic;
using BetterLife.Core.Domain.Skills;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Domain
{
    /// <summary>Tests for <see cref="SkillSet"/>.</summary>
    [TestFixture]
    public class SkillSetTests
    {
        private SkillSet _skills;
        private readonly GameDate _start = new GameDate(0);

        [SetUp]
        public void SetUp()
        {
            _skills = new SkillSet(TestCatalogs.Skills());
        }

        [Test]
        public void Practice_HasDiminishingReturns()
        {
            float first = _skills.Practice("programming", 10f, _start);
            float second = _skills.Practice("programming", 10f, _start);

            Assert.That(first, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(second, Is.EqualTo(9f).Within(0.0001f), "10 × (1 − 10/100)^1");
            Assert.That(_skills.GetLevel("programming"), Is.EqualTo(19f).Within(0.0001f));
        }

        [Test]
        public void Practice_NeverExceedsMaximum()
        {
            for (int i = 0; i < 1000; i++)
            {
                _skills.Practice("cooking", 30f, _start);
            }
            Assert.That(_skills.GetLevel("cooking"), Is.EqualTo(100f));
        }

        [Test]
        public void Decay_WaitsForGracePeriod_ThenStopsAtRetainedFloor()
        {
            _skills.Practice("programming", 40f, _start);

            _skills.ApplyWeeklyDecay(_start.AddWeeks(8));
            Assert.That(_skills.GetLevel("programming"), Is.EqualTo(40f), "within grace");

            _skills.ApplyWeeklyDecay(_start.AddWeeks(9));
            Assert.That(_skills.GetLevel("programming"), Is.EqualTo(35f).Within(0.0001f));

            for (int week = 10; week < 30; week++)
            {
                _skills.ApplyWeeklyDecay(_start.AddWeeks(week));
            }
            Assert.That(_skills.GetLevel("programming"), Is.EqualTo(20f).Within(0.0001f), "floor = peak 40 × 0.5");
            Assert.That(_skills.Find("programming").PeakLevel, Is.EqualTo(40f));
        }

        [Test]
        public void PracticeAgain_ResetsDecayClock()
        {
            _skills.Practice("cooking", 10f, _start);
            _skills.Practice("cooking", 0f, _start.AddWeeks(5));

            _skills.ApplyWeeklyDecay(_start.AddWeeks(5));

            Assert.That(_skills.GetLevel("cooking"), Is.EqualTo(10f));
        }

        [Test]
        public void InvalidInput_IsRejected()
        {
            Assert.Throws<KeyNotFoundException>(() => _skills.Practice("missing", 1f, _start));
            Assert.Throws<KeyNotFoundException>(() => _skills.GetLevel("missing"));
            Assert.Throws<ArgumentOutOfRangeException>(() => _skills.Practice("cooking", -1f, _start));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SkillDecayRule(1, 1f, 1.5f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new SkillProgress("cooking", 50f, 40f, _start));
            Assert.That(_skills.GetLevel("cooking"), Is.EqualTo(0f), "never practiced");
        }
    }
}
