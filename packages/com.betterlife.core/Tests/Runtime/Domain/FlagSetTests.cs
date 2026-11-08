using System;
using System.Collections.Generic;
using BetterLife.Core.Domain.Flags;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Domain
{
    /// <summary>Tests for <see cref="FlagSet"/>.</summary>
    [TestFixture]
    public class FlagSetTests
    {
        private FlagSet _flags;
        private readonly GameDate _now = new GameDate(100);

        [SetUp]
        public void SetUp()
        {
            _flags = new FlagSet(TestCatalogs.Flags());
        }

        [Test]
        public void BoolFlag_SetAndClear()
        {
            _flags.Set("career.fired_once", _now);

            Assert.That(_flags.IsSet("career.fired_once"), Is.True);
            Assert.That(_flags.GetCount("career.fired_once"), Is.EqualTo(1));
            Assert.That(_flags.Find("career.fired_once").ExpiresOn, Is.Null);
            Assert.That(_flags.Clear("career.fired_once"), Is.True);
            Assert.That(_flags.IsSet("career.fired_once"), Is.False);
        }

        [Test]
        public void Counter_Accumulates()
        {
            _flags.Increment("vice.gambling_wins", _now);
            int count = _flags.Increment("vice.gambling_wins", _now, 4);

            Assert.That(count, Is.EqualTo(5));
            Assert.That(_flags.IsSet("vice.gambling_wins"), Is.True);
        }

        [Test]
        public void KindMismatch_IsRejected()
        {
            Assert.Throws<InvalidOperationException>(() => _flags.Set("vice.gambling_wins", _now));
            Assert.Throws<InvalidOperationException>(() => _flags.Increment("career.fired_once", _now));
            Assert.Throws<ArgumentOutOfRangeException>(() => _flags.Increment("vice.gambling_wins", _now, 0));
        }

        [Test]
        public void Expiry_UsesDeclarationOrOverride_AndIsRemovedOnTime()
        {
            _flags.Set("relationship.recent_breakup", _now);
            _flags.Set("career.fired_once", _now, 2);

            Assert.That(_flags.RemoveExpired(_now.AddWeeks(1)), Is.Empty);
            Assert.That(_flags.RemoveExpired(_now.AddWeeks(2)), Is.EqualTo(new[] { "career.fired_once" }));
            Assert.That(_flags.IsSet("relationship.recent_breakup"), Is.True);
            Assert.That(_flags.RemoveExpired(_now.AddWeeks(26)), Is.EqualTo(new[] { "relationship.recent_breakup" }));
        }

        [Test]
        public void ResettingRefreshesExpiry()
        {
            _flags.Set("relationship.recent_breakup", _now);
            _flags.Set("relationship.recent_breakup", _now.AddWeeks(20));

            Assert.That(_flags.Find("relationship.recent_breakup").ExpiresOn, Is.EqualTo(_now.AddWeeks(46)));
        }

        [Test]
        public void UndeclaredFlags_AreRejected()
        {
            Assert.Throws<KeyNotFoundException>(() => _flags.Set("typo.flag", _now));
            Assert.Throws<KeyNotFoundException>(() => _flags.IsSet("typo.flag"));
            Assert.Throws<ArgumentOutOfRangeException>(() => _flags.Restore(new FlagEntry("career.fired_once", 0, _now, null)));
        }
    }
}
