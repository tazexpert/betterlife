using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Conditions;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Tests.Time
{
    /// <summary>
    /// Shared setup for time tests: activities, settings, a character aged 25 and an event log.
    /// </summary>
    internal sealed class TimeFixture
    {
        public static readonly GameDate Start = GameDate.FromYearWeek(2026, 0);

        public TimeFixture()
        {
            Catalogs = TestCatalogs.All();
            Character = TestCatalogs.NewCharacter(Catalogs);
            Activities = new DefinitionCatalog<ActivityDefinition>(new[]
            {
                Activity("sleep", 28, 84, 0m, null, new[] { new ActivityStatEffect(StatIds.Fatigue, -0.3f, 56) }),
                Activity("free_time", 0, 168, 0m, null, new ActivityStatEffect[0]),
                Activity("study", 0, 70, 0m, new AgeCondition(ComparisonOperator.GreaterOrEqual, 6),
                    new[] { new ActivityStatEffect(StatIds.Knowledge, 0.5f, 20) }, new[] { new ActivitySkillEffect("cooking", 1f, 10) }),
                Activity("socialize", 0, 40, 30m, null, new[] { new ActivityStatEffect(StatIds.Loneliness, -0.4f, 15) }),
                Activity("board_meeting", 0, 20, 0m, new AgeCondition(ComparisonOperator.GreaterOrEqual, 40), new ActivityStatEffect[0]),
            });
            Settings = new TimeSettings("free_time", 0.5f, 1.5f, new Dictionary<string, int> { { "sleep", 56 } });
            Bus = new EventBus();
            Random = new RandomRegistry(1UL).GetStream("time");
            State = new TimeState(new GameClock(Start), TimeBudget.CreateDefault(Activities, Settings));
            Bus.Subscribe<CashShortfallEvent>(e => Shortfalls.Add(e));
        }

        public CharacterCatalogs Catalogs { get; }

        public Character Character { get; }

        public DefinitionCatalog<ActivityDefinition> Activities { get; }

        public TimeSettings Settings { get; }

        public EventBus Bus { get; }

        public IRandomProvider Random { get; }

        public TimeState State { get; }

        public List<CashShortfallEvent> Shortfalls { get; } = new List<CashShortfallEvent>();

        public TimeBudget Budget => State.Budget;

        public RuleContext RuleContext(GameDate? week = null) => new RuleContext(Character, week ?? Start, Random, "source.test");

        private static ActivityDefinition Activity(string id, int min, int max, decimal cost, ICondition condition, ActivityStatEffect[] stats, ActivitySkillEffect[] skills = null)
        {
            return new ActivityDefinition(id, $"activity.{id}.name", $"activity.{id}.desc", "test", min, max, cost, condition, stats, skills);
        }
    }
}
