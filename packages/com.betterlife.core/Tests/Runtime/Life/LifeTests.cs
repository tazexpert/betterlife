using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Life;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Life;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Persistence;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Tests.Time;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Life
{
    /// <summary>Tests for life stages, aging, health conditions and death.</summary>
    [TestFixture]
    public class LifeTests
    {
        // Born 2000-06-15 (game week 23 of 2000).
        private static readonly GameDate Birthday25 = GameDate.FromYearWeek(2025, 23);

        [TestCase(0, "baby")]
        [TestCase(5, "baby")]
        [TestCase(6, "child")]
        [TestCase(17, "child")]
        [TestCase(18, "adult")]
        [TestCase(99, "senior")]
        public void StageFor_UsesStartAges(int age, string expected)
        {
            Assert.That(TestCatalogs.LifeStages().StageFor(age).Id, Is.EqualTo(expected));
        }

        [Test]
        public void StageCatalog_RequiresAStageAtZero_AndUniqueAges()
        {
            Assert.Throws<ArgumentException>(() => new LifeStageCatalog(new[] { new LifeStageDefinition("a", "a.name", 1, true, 0f, null, null) }));
            Assert.Throws<ArgumentException>(() => new LifeStageCatalog(new[]
            {
                new LifeStageDefinition("a", "a.name", 0, true, 0f, null, null), new LifeStageDefinition("b", "b.name", 0, true, 0f, null, null),
            }));
        }

        [Test]
        public void Birthday_OnlyInTheBirthdayWeek_AppliesStageEffects()
        {
            var f = new TimeFixture();
            var birthdays = new List<int>();
            f.Bus.Subscribe<BirthdayEvent>(e => birthdays.Add(e.Age));
            AgingPhase phase = Aging(f);
            float fitness = f.Character.Stats.GetValue(StatIds.Fitness);

            phase.Execute(new TickContext(Birthday25.AddWeeks(-1), f.Character));
            phase.Execute(new TickContext(Birthday25, f.Character));
            phase.Execute(new TickContext(f.Character.Birth.Week, f.Character));

            Assert.That(birthdays, Is.EqualTo(new[] { 25 }));
            Assert.That(f.Character.Stats.GetValue(StatIds.Fitness), Is.EqualTo(fitness - 1f));
            Assert.That(f.Character.ChangeLog.Entries.Last().SourceKey, Is.EqualTo("stage.adult.name"));
        }

        [Test]
        public void EnteringAStage_ResetsTheWeeklyPlan_AndInterrupts()
        {
            var f = new TimeFixture();
            var changes = new List<LifeStageChangedEvent>();
            f.Bus.Subscribe<LifeStageChangedEvent>(e => changes.Add(e));
            var settings = new TimeSettings("free_time", 0.5f, 1.5f, new Dictionary<string, int> { { "sleep", 56 } },
                new Dictionary<string, IReadOnlyDictionary<string, int>> { { "adult", new Dictionary<string, int> { { "sleep", 63 }, { "study", 10 } } } });
            var phase = new AgingPhase(f.State, settings, new Pcg32(1UL, 1UL), f.Bus);
            var tick = new TickContext(GameDate.FromYearWeek(2018, 23), f.Character);

            phase.Execute(tick);

            Assert.That(changes.Single().Previous.Id, Is.EqualTo("child"));
            Assert.That(changes.Single().Current.Id, Is.EqualTo("adult"));
            Assert.That(f.Budget.GetHours("sleep"), Is.EqualTo(63));
            Assert.That(f.Budget.GetHours("study"), Is.EqualTo(10));
            Assert.That(tick.Interrupts.Single().Severity, Is.EqualTo(Severity.Normal));
        }

        [Test]
        public void SeniorBirthday_RollsAgeRelatedConditions()
        {
            var f = new TimeFixture();
            var started = new List<HealthConditionStartedEvent>();
            f.Bus.Subscribe<HealthConditionStartedEvent>(e => started.Add(e));
            f.Character.Stats.SetStored(StatIds.DiseaseBurden, 100f);

            Aging(f).Execute(new TickContext(GameDate.FromYearWeek(2061, 23), f.Character));

            Assert.That(f.Character.HasCondition("cold"), Is.True, "risk 1 × health factor above 1 is certain");
            Assert.That(started.Single().Condition.Severity, Is.InRange(30f, 70f));
        }

        [Test]
        public void Conditions_ApplyWeeklyEffects_AndMayRecover()
        {
            var f = new TimeFixture();
            f.Character.SetCondition(new HealthCondition("flu", 50f, TimeFixture.Start));
            f.Character.SetCondition(new HealthCondition("cold", 80f, TimeFixture.Start));
            var recovered = new List<string>();
            f.Bus.Subscribe<HealthConditionRecoveredEvent>(e => recovered.Add(e.Condition.Id));
            float fatigue = f.Character.Stats.GetValue(StatIds.Fatigue);

            new HealthConditionPhase(new Pcg32(2UL, 2UL), f.Bus).Execute(new TickContext(TimeFixture.Start.AddWeeks(1), f.Character));

            Assert.That(f.Character.Stats.GetValue(StatIds.Fatigue), Is.EqualTo(fatigue + 5f), "10 × severity 50%");
            Assert.That(recovered, Is.EqualTo(new[] { "cold" }));
            Assert.That(f.Character.HasCondition("flu"), Is.True, "chronic");
        }

        [Test]
        public void Mortality_ComposesAgeHealthHealthcareStageAndConditions()
        {
            var model = new MortalityModel(Settings());
            Character c = TestCatalogs.NewCharacter();
            c.MoveTo(new BetterLife.Core.Domain.World.Residence("thailand", "thailand.rural"));
            GameDate at30 = GameDate.FromYearWeek(2030, 30);

            MortalityBreakdown baseline = model.Explain(c, at30);
            Assert.That(baseline.Age, Is.EqualTo(30));
            Assert.That(baseline.AgeRisk, Is.EqualTo(0.001f).Within(1e-6f));
            Assert.That(baseline.HealthcareMultiplier, Is.EqualTo((float)Math.Exp(Math.Log(0.85) * 0.3)).Within(1e-4f), "healthcare 65");
            Assert.That(model.Explain(c, GameDate.FromYearWeek(2070, 30)).AgeRisk, Is.EqualTo(0.001f * (float)Math.Exp(4.0)).Within(1e-5f));

            c.Stats.SetStored(StatIds.Fitness, 0f);
            c.Stats.SetStored(StatIds.Nutrition, 0f);
            c.Stats.SetStored(StatIds.Fatigue, 100f);
            c.Stats.SetStored(StatIds.DiseaseBurden, 100f);
            Assert.That(model.Explain(c, at30).HealthMultiplier, Is.EqualTo(8f).Within(1e-3f), "Health 0");

            c.SetCondition(new HealthCondition("flu", 4f, at30));
            MortalityBreakdown ill = model.Explain(c, at30);
            Assert.That(ill.ConditionRisk, Is.EqualTo(0.02f).Within(1e-6f), "0.5 × severity 4%");
            Assert.That(ill.Annual, Is.LessThan(1f));
            Assert.That(ill.Weekly, Is.EqualTo((float)(1 - Math.Pow(1 - ill.Annual, 1.0 / 52))).Within(1e-6f));
            Assert.That(model.Explain(c, GameDate.FromYearWeek(2001, 30)).StageRisk, Is.EqualTo(0.01f), "baby stage");
        }

        [Test]
        public void Mortality_IsCertainAtMaxAge()
        {
            var model = new MortalityModel(Settings());

            Assert.That(model.WeeklyProbability(TestCatalogs.NewCharacter(), GameDate.FromYearWeek(2100, 30)), Is.EqualTo(1f));
        }

        [Test]
        public void DeathCheck_KillsWithAWeightedCause_AndInterruptsCritically()
        {
            var bus = new EventBus();
            var died = new List<CharacterDiedEvent>();
            bus.Subscribe<CharacterDiedEvent>(e => died.Add(e));
            using (DeathService deaths = Deaths(bus))
            {
                Character c = TestCatalogs.NewCharacter();
                var tick = new TickContext(GameDate.FromYearWeek(2100, 30), c);

                new DeathCheckPhase(new MortalityModel(Settings()), deaths, new Pcg32(3UL, 3UL)).Execute(tick);

                Assert.That(c.IsAlive, Is.False);
                Assert.That(c.Death.AgeYears, Is.EqualTo(100));
                Assert.That(died.Single().Death.CauseId, Is.EqualTo(c.Death.CauseId));
                Assert.That(tick.Interrupts.Single().Severity, Is.EqualTo(Severity.Critical));
                Assert.Throws<InvalidOperationException>(() => c.Die("natural", tick.Week));
            }
        }

        [Test]
        public void Causes_FollowEligibilityAndWeights()
        {
            using (DeathService deaths = Deaths(new EventBus()))
            {
                Character c = TestCatalogs.NewCharacter();
                var random = new Pcg32(4UL, 4UL);

                var young = Enumerable.Range(0, 500).Select(_ => deaths.PickCause(c, GameDate.FromYearWeek(2030, 30), random).Id).ToList();
                var old = Enumerable.Range(0, 2000).Select(_ => deaths.PickCause(c, GameDate.FromYearWeek(2085, 30), random).Id).ToList();

                Assert.That(young, Is.All.EqualTo("natural"), "old age needs age 60 and accident has weight 0");
                Assert.That(old.Count(id => id == "old_age") / 2000.0, Is.EqualTo(50.0 / 51.0).Within(0.02));
            }
        }

        [Test]
        public void DeathRequest_FromAnEffect_EndsTheLife_AndTimeStops()
        {
            var f = new TimeFixture();
            using (DeathService deaths = Deaths(f.Bus))
            {
                var phase = new DelegatePhase(TickOrder.EventRoll, t =>
                {
                    if (t.Week == TimeFixture.Start.AddWeeks(2))
                    {
                        new DieEffect("accident").Apply(new RuleContext(t.Character, t.Week, f.Random, "source.test")).PublishTo(f.Bus);
                    }
                });
                var counter = new DelegatePhase(TickOrder.Record, _ => { });
                var system = new TimeSystem(f.State.Clock, new ITickPhase[] { phase, counter }, f.Bus, new InterruptPolicy());

                AdvanceResult result = system.AdvanceWeeks(f.Character, 52);
                AdvanceResult after = system.AdvanceWeeks(f.Character, 10);

                Assert.That(f.Character.Death.CauseId, Is.EqualTo("accident"));
                Assert.That(result.WeeksAdvanced, Is.EqualTo(2));
                Assert.That(result.StoppedBy.Severity, Is.EqualTo(Severity.Critical));
                Assert.That(after.WeeksAdvanced, Is.EqualTo(0), "a dead character does not age");
            }
        }

        [Test]
        public void Death_IsSavedAndRestored()
        {
            Character c = TestCatalogs.NewCharacter();
            c.Die("natural", GameDate.FromYearWeek(2070, 10));

            Character copy = CharacterDtoMapper.FromDto(CharacterDtoMapper.ToDto(c), TestCatalogs.All()).Character;

            Assert.That(copy.IsAlive, Is.False);
            Assert.That((copy.Death.CauseId, copy.Death.Week, copy.Death.AgeYears), Is.EqualTo(("natural", GameDate.FromYearWeek(2070, 10), 69)));
        }

        [Test]
        public void LifeStageCondition_ReadsTheCurrentStage()
        {
            Character c = TestCatalogs.NewCharacter();
            var adult = new LifeStageCondition(new[] { "adult" });

            Assert.That(adult.Evaluate(new RuleContext(c, GameDate.FromYearWeek(2030, 1), new Pcg32(1UL, 1UL), "source.test")), Is.True);
            Assert.That(adult.Evaluate(new RuleContext(c, GameDate.FromYearWeek(2010, 1), new Pcg32(1UL, 1UL), "source.test")), Is.False);
        }

        [Test]
        public void TimeBudget_Reset_ReplacesThePlan_SkippingInvalidEntries()
        {
            var f = new TimeFixture();
            f.Budget.Set("study", 20);

            f.Budget.Reset(new Dictionary<string, int> { { "sleep", 70 }, { "socialize", 99 }, { "free_time", 5 }, { "unknown", 3 } });

            Assert.That(f.Budget.Allocations, Is.EquivalentTo(new Dictionary<string, int> { { "sleep", 70 } }));
        }

        private static MortalitySettings Settings() => new MortalitySettings(30, 0.001f, 0.1f, 100, 8f, 0.6f, 1.4f, 0.85f, 0.5f);

        private static DeathService Deaths(IEventBus bus)
        {
            var always = new EventWeight(1f, null);
            var causes = new DefinitionCatalog<DeathCauseDefinition>(new[]
            {
                new DeathCauseDefinition("natural", "death.natural.name", null, null, always),
                new DeathCauseDefinition("old_age", "death.old_age.name", null, new BetterLife.Core.Rules.Conditions.AgeCondition(ComparisonOperator.GreaterOrEqual, 60),
                    new EventWeight(1f, new[] { new WeightModifier(new BetterLife.Core.Rules.Conditions.AgeCondition(ComparisonOperator.GreaterOrEqual, 80), 50f) })),
                new DeathCauseDefinition("accident", "death.accident.name", null, null, new EventWeight(0f, null)),
            });
            return new DeathService(causes, bus);
        }

        private static AgingPhase Aging(TimeFixture f) => new AgingPhase(f.State, f.Settings, new Pcg32(1UL, 1UL), f.Bus);

        private sealed class DelegatePhase : ITickPhase
        {
            private readonly Action<TickContext> _action;

            public DelegatePhase(int order, Action<TickContext> action)
            {
                Order = order;
                _action = action;
                Name = "delegate_" + order;
            }

            public string Name { get; }

            public int Order { get; }

            public TickCadence Cadence => TickCadence.Weekly;

            public void Execute(TickContext context) => _action(context);
        }
    }
}
