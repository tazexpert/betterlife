using System.Linq;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Randomness;
using NUnit.Framework;

namespace BetterLife.Core.Tests.LifeEvents
{
    /// <summary>Tests for life milestones (chance within an age range, once per life, modifiers).</summary>
    [TestFixture]
    public class MilestoneTests
    {
        // The fixture character is 25-26 years old during the test weeks.
        private static string Milestone(string id, int minAge, int maxAge, float chance, string modifiers = "")
        {
            return $@"{{ 'id': '{id}', 'titleKey': 'event.{id}.title', 'bodyKey': 'event.{id}.body', 'category': 'test', 'trigger': 'milestone',
                'severity': 'minor', 'tone': 'neutral', 'milestone': {{ 'minAge': {minAge}, 'maxAge': {maxAge}, 'chance': {chance} {modifiers} }},
                'onFire': [ {{ 'type': 'flag.set', 'flag': 'career.fired_once' }} ], 'choices': [] }}";
        }

        [Test]
        public void WeeklyShare_AddsUpToTheOverallChance()
        {
            Assert.That(MilestoneChance.WeeklyChance(100f, 300), Is.EqualTo(1f));
            Assert.That(MilestoneChance.WeeklyChance(0f, 300), Is.EqualTo(0f));

            const int weeks = 6 * 52;
            float weekly = MilestoneChance.WeeklyChance(25f, weeks);
            var random = new Pcg32(7UL, 3UL);
            int lives = 20000;
            int happened = 0;
            for (int i = 0; i < lives; i++)
            {
                for (int w = 0; w < weeks; w++)
                {
                    if (random.Chance(weekly))
                    {
                        happened++;
                        break;
                    }
                }
            }
            Assert.That(happened / (float)lives, Is.EqualTo(0.25f).Within(0.015f), "a full age range meets the milestone with the given chance");
        }

        [Test]
        public void CertainMilestone_HappensOnce_InTheFirstWeek()
        {
            var f = new EventFixture(EventFixture.Events(Milestone("test.sure", 20, 30, 100)), weeklyChance: 0f);
            f.RunWeek();
            Assert.That(f.Fired.Select(e => e.Definition.Id), Is.EqualTo(new[] { "test.sure" }));
            for (int i = 0; i < 20; i++)
            {
                f.RunWeek();
            }
            Assert.That(f.Fired.Count, Is.EqualTo(1), "once per life");
        }

        [Test]
        public void Milestone_OutsideItsAgeRange_NeverHappens()
        {
            var f = new EventFixture(EventFixture.Events(Milestone("test.later", 40, 50, 100)), weeklyChance: 0f);
            for (int i = 0; i < 52; i++)
            {
                f.RunWeek();
            }
            Assert.That(f.Fired, Is.Empty);
        }

        [Test]
        public void Modifiers_RaiseTheChance_WhileTheirConditionHolds()
        {
            string modifiers = ", 'modifiers': [ { 'condition': { 'type': 'flag', 'flag': 'vice.wins' }, 'add': 100 } ]";
            var f = new EventFixture(EventFixture.Events(Milestone("test.followup", 20, 30, 0, modifiers)), weeklyChance: 0f);
            for (int i = 0; i < 10; i++)
            {
                f.RunWeek();
            }
            Assert.That(f.Fired, Is.Empty, "0% without the earlier milestone");

            f.Character.Flags.Increment("vice.wins", f.Clock.Now);
            f.RunWeek();
            Assert.That(f.Fired.Single().Definition.Id, Is.EqualTo("test.followup"), "+100 points once the flag is set");
        }

        [Test]
        public void MilestoneEvents_AreNotPartOfTheWeeklyRandomDraw()
        {
            var f = new EventFixture(EventFixture.Events(Milestone("test.never", 40, 50, 100)), weeklyChance: 1f);
            for (int i = 0; i < 52; i++)
            {
                f.RunWeek();
            }
            Assert.That(f.Fired, Is.Empty);
        }
    }
}
