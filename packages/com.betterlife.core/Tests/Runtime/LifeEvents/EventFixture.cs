using System.Collections.Generic;
using BetterLife.Core.Astrology;
using BetterLife.Core.Content;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Events;
using BetterLife.Core.Karma;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Tests.Content;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Tests.LifeEvents
{
    /// <summary>
    /// An event system over in-memory content with the given events, plus a character and helpers to run weeks.
    /// </summary>
    internal sealed class EventFixture
    {
        public static readonly GameDate Start = GameDate.FromYearWeek(2026, 0);

        public EventFixture(string eventsJson, float weeklyChance = 1f, ulong seed = 11UL)
        {
            InMemoryContentSource source = TestContent.Source("{ 'type': 'events', 'path': 'events/test.json' }", weeklyChance)
                .Add("events/test.json", eventsJson);
            Content = TestContent.Load(source);
            Character = new Character(new CharacterId("chr_00000000000000ee"), new PersonName("Test", "Person"),
                GenderProfile.DefaultFor(BiologicalSex.Female), new BirthInfo(new CalendarDate(2000, 6, 15), 9, "thailand"),
                Content.Get<CharacterCatalogs>());
            Bus = new EventBus();
            Clock = new GameClock(Start);
            Random = new RandomRegistry(seed);
            Luck = new LuckCalculator(Content.Get<LuckSettings>(), Content.Get<FortuneCalculator>(), Content.Get<KarmaService>());
            State = new EventState();
            System = EventSystemFactory.Create(Content, State, Luck, Random, Bus, Clock);
            Bus.Subscribe<LifeEventFiredEvent>(e => Fired.Add(e));
            Bus.Subscribe<CashShortfallEvent>(e => Shortfalls.Add(e));
        }

        public IContentRepository Content { get; }

        public Character Character { get; }

        public EventBus Bus { get; }

        public GameClock Clock { get; }

        public RandomRegistry Random { get; }

        public LuckCalculator Luck { get; }

        public EventState State { get; }

        public EventSystem System { get; }

        public List<LifeEventFiredEvent> Fired { get; } = new List<LifeEventFiredEvent>();

        public List<CashShortfallEvent> Shortfalls { get; } = new List<CashShortfallEvent>();

        /// <summary>Runs the event step for the next week and moves the clock.</summary>
        public TickContext RunWeek()
        {
            var tick = new TickContext(Clock.Now.AddWeeks(1), Character);
            System.RunWeek(tick);
            Clock.MoveTo(tick.Week);
            return tick;
        }

        public static string Events(params string[] events) => "{ 'events': [ " + string.Join(", ", events) + " ] }";

        public static string Simple(string id, string extra = "", string trigger = "random", string tone = "neutral", string severity = "minor")
        {
            return $"{{ 'id': '{id}', 'titleKey': 'event.{id}.title', 'bodyKey': 'event.{id}.body', 'category': 'test', "
                + $"'trigger': '{trigger}', 'severity': '{severity}', 'tone': '{tone}', 'choices': [] {extra} }}";
        }
    }
}
