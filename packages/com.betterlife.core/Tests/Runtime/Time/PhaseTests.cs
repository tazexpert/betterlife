using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.Time.Ticking.Phases;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Time
{
    /// <summary>Tests for the built-in tick phases.</summary>
    [TestFixture]
    public class PhaseTests
    {
        private TimeFixture _f;
        private TickContext _tick;

        [SetUp]
        public void SetUp()
        {
            _f = new TimeFixture();
            _tick = new TickContext(TimeFixture.Start.AddWeeks(1), _f.Character);
        }

        [Test]
        public void Allocation_AppliesHourlyEffects_WithCaps_AndSourceKeys()
        {
            _f.Character.Stats.SetStored(StatIds.Fatigue, 50f);
            _f.Budget.Set("study", 30);
            TimeAllocationAppliedEvent applied = null;
            _f.Bus.Subscribe<TimeAllocationAppliedEvent>(e => applied = e);

            Phase().Execute(_tick);

            Assert.That(_f.Character.Stats.GetValue(StatIds.Fatigue), Is.EqualTo(50f - 0.3f * 56).Within(0.001f));
            Assert.That(_f.Character.Stats.GetValue(StatIds.Knowledge), Is.EqualTo(10f).Within(0.001f), "capped at 20 hours × 0.5");
            Assert.That(_f.Character.ChangeLog.Entries.Select(e => e.SourceKey), Has.Member("activity.study.name"));
            Assert.That(applied.Hours["study"], Is.EqualTo(30));
            Assert.That(applied.Hours["free_time"], Is.EqualTo(82));
        }

        [Test]
        public void Allocation_SkillGain_UsesMentalLearningMultiplier()
        {
            _f.Budget.Set("study", 10);
            float mental = _f.Character.Stats.GetValue(StatIds.Mental);

            Phase().Execute(_tick);

            Assert.That(_f.Character.Skills.GetLevel("cooking"), Is.EqualTo(10f * _f.Settings.LearningMultiplier(mental)).Within(0.001f));
        }

        [Test]
        public void Allocation_ChargesCosts_AndPublishesShortfall()
        {
            _f.Character.Wallet.Deposit(100m);
            _f.Budget.Set("socialize", 5);

            Phase().Execute(_tick);

            Assert.That(_f.Character.Wallet.Cash, Is.EqualTo(0m));
            Assert.That(_f.Shortfalls.Single().Amount, Is.EqualTo(50m));
            Assert.That(_f.Shortfalls.Single().SourceKey, Is.EqualTo("activity.socialize.name"));
        }

        [Test]
        public void Decay_MovesTowardTarget_WithoutOvershooting()
        {
            _f.Character.Stats.SetStored(StatIds.Fitness, 31f);
            _f.Character.Stats.SetStored(StatIds.Stress, 50f);

            new NaturalDecayPhase().Execute(_tick);

            Assert.That(_f.Character.Stats.GetValue(StatIds.Fitness), Is.EqualTo(30f).Within(0.0001f), "gap 1 < rate 2");
            Assert.That(_f.Character.Stats.GetValue(StatIds.Stress), Is.EqualTo(48.5f).Within(0.0001f));
            Assert.That(_f.Character.ChangeLog.Entries.First().SourceKey, Is.EqualTo(NaturalDecayPhase.SourceKey));
        }

        [Test]
        public void Decay_RespectsTraitMultipliers_AndNeverPassesTheTarget()
        {
            _f.Character.Traits.Add("athletic", TimeFixture.Start);
            _f.Character.Stats.SetStored(StatIds.Fitness, 50f);

            new NaturalDecayPhase().Execute(_tick);
            Assert.That(_f.Character.Stats.GetValue(StatIds.Fitness), Is.EqualTo(48.4f).Within(0.0001f), "loss 2 × 0.8");

            _f.Character.Traits.Add("sensitive", TimeFixture.Start);
            _f.Character.Stats.SetStored(StatIds.Stress, 21f);
            new NaturalDecayPhase().Execute(_tick);
            Assert.That(_f.Character.Stats.GetValue(StatIds.Stress), Is.EqualTo(20f).Within(0.0001f), "× 2 would overshoot");
        }

        [Test]
        public void Decay_AlsoFadesSkills_AndExpiresFlags()
        {
            _f.Character.Skills.Practice("cooking", 10f, TimeFixture.Start.AddWeeks(-20));
            _f.Character.Flags.Set("relationship.recent_breakup", TimeFixture.Start.AddWeeks(-25));

            new NaturalDecayPhase().Execute(_tick);

            Assert.That(_f.Character.Skills.GetLevel("cooking"), Is.EqualTo(9f).Within(0.0001f));
            Assert.That(_f.Character.Flags.IsSet("relationship.recent_breakup"), Is.False);
        }

        [Test]
        public void Record_SamplesCoreStats()
        {
            new WeeklyRecordPhase().Execute(_tick);

            Assert.That(_f.Character.History.LastRecordedWeek, Is.EqualTo(_tick.Week));
        }

        [Test]
        public void StandardPhases_UseTheDesignOrder()
        {
            var phases = new List<ITickPhase> { new WeeklyRecordPhase(), new NaturalDecayPhase(), Phase() };

            var system = new TimeSystem(_f.State.Clock, phases, _f.Bus, new InterruptPolicy());

            Assert.That(system.Phases.Select(p => p.Name), Is.EqualTo(new[] { "time_allocation", "natural_decay", "weekly_record" }));
        }

        private TimeAllocationPhase Phase() => new TimeAllocationPhase(_f.State, _f.Activities, _f.Settings, _f.Random, _f.Bus);
    }
}
