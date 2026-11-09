using System.Collections.Generic;
using BetterLife.Core.Astrology;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Karma;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Relationships;
using BetterLife.Core.Tests.Careers;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.Time.Ticking.Phases;
using BetterLife.Core.Vices;

namespace BetterLife.Core.Tests.Vices
{
    /// <summary>
    /// A character born in mid-2000 with a vice service built from the test content.
    /// </summary>
    internal sealed class ViceFixture
    {
        public ViceFixture(int year = 2030, ulong seed = 7UL)
        {
            IContentRepository content = SchoolWorkFixture.Content;
            Character = TestCatalogs.NewCharacter();
            Clock = new GameClock(GameDate.FromYearWeek(year, 0));
            Bus = new EventBus();
            Settings = content.Get<ViceSettings>();
            var luck = new LuckCalculator(content.Get<LuckSettings>(), content.Get<FortuneCalculator>(), content.Get<KarmaService>());
            Service = new ViceService(Character, content.Get<DefinitionCatalog<ViceDefinition>>(), content.Get<DefinitionCatalog<QuitMethodDefinition>>(),
                Settings, luck, new RandomRegistry(seed).GetStream("vices"), Bus, Clock);
        }

        public Character Character { get; }

        public GameClock Clock { get; }

        public EventBus Bus { get; }

        public ViceSettings Settings { get; }

        public ViceService Service { get; }

        public TickContext LastTick { get; private set; }

        public ViceState State(string viceId) => Character.Vices.Find(viceId);

        public float Stat(StatId id) => Character.Stats.GetValue(id);

        public void ChangeStat(StatId id, float delta) => Character.ApplyStatChange(new StatChange(id, delta, "test.source"), Clock.Now);

        /// <summary>Publishes this week's hours, runs the weekly vice step and moves one week on.</summary>
        public void Week(Dictionary<string, int> hours = null)
        {
            Bus.Publish(new TimeAllocationAppliedEvent(Clock.Now, hours ?? new Dictionary<string, int>(), new StatChangeResult[0], 0m));
            LastTick = new TickContext(Clock.Now, Character);
            Service.ProcessWeek(LastTick);
            Clock.MoveTo(Clock.Now.AddWeeks(1));
        }

        /// <summary>Adds an NPC relationship.</summary>
        public Relationship Add(RelationshipType type, float value, params string[] traits)
        {
            return Character.Relationships.Add(id => new Npc(id, new PersonName("N" + id, "T"), BiologicalSex.Female, Clock.Now.AddWeeks(-1500),
                new Genome(), traits), type, value, Clock.Now);
        }
    }
}
