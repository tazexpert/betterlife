using System;
using System.Collections.Generic;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.Traits;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Domain
{
    /// <summary>Tests for <see cref="TraitSet"/>.</summary>
    [TestFixture]
    public class TraitSetTests
    {
        private TraitSet _traits;
        private readonly GameDate _now = new GameDate(100);

        [SetUp]
        public void SetUp()
        {
            _traits = new TraitSet(TestCatalogs.Traits());
        }

        [Test]
        public void Add_RespectsExclusiveGroups_AndDuplicates()
        {
            Assert.That(_traits.Add("anxious", _now), Is.EqualTo(TraitAddOutcome.Added));
            Assert.That(_traits.Add("anxious", _now), Is.EqualTo(TraitAddOutcome.AlreadyHas));
            Assert.That(_traits.Add("calm", _now), Is.EqualTo(TraitAddOutcome.ExclusiveConflict));
            Assert.That(_traits.Add("athletic", _now), Is.EqualTo(TraitAddOutcome.Added));
            Assert.That(_traits.Instances, Has.Count.EqualTo(2));
        }

        [Test]
        public void HiddenTraits_StartHidden_AndCanBeRevealedOnce()
        {
            _traits.Add("anxious", _now);
            _traits.Add("athletic", _now);

            Assert.That(_traits.Instances[0].IsRevealed, Is.False);
            Assert.That(_traits.Instances[1].IsRevealed, Is.True);
            Assert.That(_traits.Reveal("anxious"), Is.True);
            Assert.That(_traits.Reveal("anxious"), Is.False);
            Assert.That(_traits.Reveal("calm"), Is.False, "not held");
        }

        [Test]
        public void Multiplier_DependsOnDirection_AndMultipliesAcrossTraits()
        {
            _traits.Add("anxious", _now);
            _traits.Add("sensitive", _now);
            _traits.Add("athletic", _now);

            Assert.That(_traits.GetMultiplier(StatIds.Stress, 5f), Is.EqualTo(2.5f).Within(0.0001f));
            Assert.That(_traits.GetMultiplier(StatIds.Stress, -5f), Is.EqualTo(2f).Within(0.0001f));
            Assert.That(_traits.GetMultiplier(StatIds.Fitness, 5f), Is.EqualTo(1.2f).Within(0.0001f));
            Assert.That(_traits.GetMultiplier(StatIds.Fitness, -5f), Is.EqualTo(0.8f).Within(0.0001f));
            Assert.That(_traits.GetMultiplier(StatIds.Happiness, 5f), Is.EqualTo(1f));
            Assert.That(_traits.GetMultiplier(StatIds.Stress, 0f), Is.EqualTo(1f));
        }

        [Test]
        public void Remove_FreesExclusiveGroup()
        {
            _traits.Add("anxious", _now);

            Assert.That(_traits.Remove("anxious"), Is.True);
            Assert.That(_traits.Remove("anxious"), Is.False);
            Assert.That(_traits.Add("calm", _now), Is.EqualTo(TraitAddOutcome.Added));
        }

        [Test]
        public void UnknownTraits_AndInvalidModifiers_AreRejected()
        {
            Assert.Throws<KeyNotFoundException>(() => _traits.Add("missing", _now));
            Assert.Throws<KeyNotFoundException>(() => _traits.Restore(new TraitInstance("missing", _now, true)));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TraitStatModifier(StatIds.Stress, ModifierDirection.Gain, 0f));
        }
    }
}
