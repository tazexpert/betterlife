using System;
using System.Linq;
using BetterLife.Core.Time.Budget;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Time
{
    /// <summary>Tests for <see cref="TimeBudget"/> and <see cref="TimeSettings"/>.</summary>
    [TestFixture]
    public class TimeBudgetTests
    {
        private TimeFixture _f;

        [SetUp]
        public void SetUp()
        {
            _f = new TimeFixture();
        }

        [Test]
        public void DefaultBudget_LeavesTheRestAsFreeTime()
        {
            Assert.That(_f.Budget.GetHours("sleep"), Is.EqualTo(56));
            Assert.That(_f.Budget.FreeHours, Is.EqualTo(112));
            Assert.That(_f.Budget.GetHours("free_time"), Is.EqualTo(112));
            Assert.That(_f.Budget.Allocations.ContainsKey("free_time"), Is.False);
        }

        [Test]
        public void Set_RespectsActivityMaximumAndTheWeek()
        {
            Assert.That(_f.Budget.Set("socialize", 41), Is.False, "above max");
            Assert.That(_f.Budget.Set("study", 70), Is.True);
            Assert.That(_f.Budget.Set("socialize", 40), Is.True);
            Assert.That(_f.Budget.Set("sleep", 84), Is.False, "would exceed 168");
            Assert.That(_f.Budget.FreeHours, Is.EqualTo(2));
            Assert.That(_f.Budget.Set("socialize", 0), Is.True);
            Assert.That(_f.Budget.Allocations.ContainsKey("socialize"), Is.False);
        }

        [Test]
        public void Set_RejectsDefaultActivity_NegativeHours_AndUnknownIds()
        {
            Assert.Throws<InvalidOperationException>(() => _f.Budget.Set("free_time", 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => _f.Budget.Set("study", -1));
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => _f.Budget.Set("teleporting", 1));
        }

        [Test]
        public void Validate_ReportsMinimum_Unavailable_AndOverWeek()
        {
            var budget = new TimeBudget(_f.Activities, _f.Settings);
            budget.Restore("board_meeting", 10);
            budget.Restore("study", 100);
            budget.Restore("socialize", 80);

            var issues = budget.Validate(_f.RuleContext());

            Assert.That(issues.Select(i => (i.Kind, i.ActivityId)), Is.EquivalentTo(new[]
            {
                (BudgetIssueKind.OverWeek, (string)null),
                (BudgetIssueKind.BelowMinimum, "sleep"),
                (BudgetIssueKind.AboveMaximum, "study"),
                (BudgetIssueKind.AboveMaximum, "socialize"),
                (BudgetIssueKind.Unavailable, "board_meeting"),
            }));
            Assert.That(_f.Budget.Validate(_f.RuleContext()), Is.Empty);
        }

        [Test]
        public void EffectiveAllocation_MovesUnavailableHoursToFreeTime_AndAlwaysSumsToAWeek()
        {
            var budget = new TimeBudget(_f.Activities, _f.Settings);
            budget.Restore("sleep", 84);
            budget.Restore("board_meeting", 20);
            budget.Restore("study", 70);
            budget.Restore("socialize", 40);

            var effective = budget.EffectiveAllocation(_f.RuleContext());

            Assert.That(effective.Values.Sum(), Is.EqualTo(TimeBudget.HoursPerWeek));
            Assert.That(effective.ContainsKey("board_meeting"), Is.False);
            Assert.That(effective["sleep"], Is.EqualTo(84));
            Assert.That(effective["study"], Is.EqualTo(70));
            Assert.That(effective["socialize"], Is.EqualTo(14), "trimmed to fit the week");
            Assert.That(effective["free_time"], Is.EqualTo(0));
        }

        [TestCase(0f, 0.5f)]
        [TestCase(50f, 1f)]
        [TestCase(100f, 1.5f)]
        [TestCase(150f, 1.5f)]
        public void LearningMultiplier_FollowsMental(float mental, float expected)
        {
            Assert.That(_f.Settings.LearningMultiplier(mental), Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void Definitions_ValidateInput()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new ActivityDefinition("a", "a.name", "a.desc", "x", 10, 5, 0m, null, null, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ActivityDefinition("a", "a.name", "a.desc", "x", 0, 169, 0m, null, null, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ActivityDefinition("a", "a.name", "a.desc", "x", 0, 5, -1m, null, null, null));
            Assert.Throws<ArgumentOutOfRangeException>(() => new ActivitySkillEffect("cooking", -1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new TimeSettings("free_time", 0f, 1f, null));
            Assert.That(new ActivityStatEffect(Core.Domain.Stats.StatIds.Fitness, 0.5f, 10).TotalFor(30), Is.EqualTo(5f));
        }
    }
}
