using System.Collections.Generic;
using BetterLife.Core.Astrology;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Events;
using BetterLife.Core.Karma;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Relationships;
using BetterLife.Core.Scams;
using BetterLife.Core.Tests.Careers;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.Time.Ticking.Phases;

namespace BetterLife.Core.Tests.Scams
{
    /// <summary>
    /// A character born in mid-2000 with a scam service built from the test content.
    /// </summary>
    internal sealed class ScamFixture
    {
        public ScamFixture(int year = 2030, ulong seed = 13UL)
        {
            IContentRepository content = SchoolWorkFixture.Content;
            Character = TestCatalogs.NewCharacter();
            Clock = new GameClock(GameDate.FromYearWeek(year, 0));
            Bus = new EventBus();
            Settings = content.Get<ScamSettings>();
            var luck = new LuckCalculator(content.Get<LuckSettings>(), content.Get<FortuneCalculator>(), content.Get<KarmaService>());
            Service = new ScamService(Character, content.Get<DefinitionCatalog<ScamDefinition>>(), Settings, content.Get<DefinitionCatalog<AssetDefinition>>(),
                luck, new RandomRegistry(seed).GetStream("scams"), Bus, Clock);
        }

        public Character Character { get; }

        public GameClock Clock { get; }

        public EventBus Bus { get; }

        public ScamSettings Settings { get; }

        public ScamService Service { get; }

        public TickContext LastTick { get; private set; }

        public float Stat(StatId id) => Character.Stats.GetValue(id);

        public void ChangeStat(StatId id, float delta) => Character.ApplyStatChange(new StatChange(id, delta, "test.source"), Clock.Now);

        /// <summary>Makes the phone gang target the character half the months (fired flag ×3 + over 60 ×2.5, capped).</summary>
        public void MakeTarget() => Character.Flags.Set("career.fired_once", Clock.Now);

        public Holding Deposit(decimal amount) => Character.Finances.Portfolio.Add("savings_account", amount, Clock.Now);

        public void Hours(int socialize) =>
            Bus.Publish(new TimeAllocationAppliedEvent(Clock.Now, new Dictionary<string, int> { ["socialize"] = socialize }, new StatChangeResult[0], 0m));

        /// <summary>Runs the monthly scam step at the current week and moves four weeks on.</summary>
        public void Month()
        {
            LastTick = new TickContext(Clock.Now, Character);
            Service.ProcessMonth(LastTick);
            Clock.MoveTo(Clock.Now.AddWeeks(4));
        }

        public Relationship Add(RelationshipType type, float value)
        {
            return Character.Relationships.Add(id => new Npc(id, new PersonName("N" + id, "T"), BiologicalSex.Female, Clock.Now.AddWeeks(-1500),
                new Genome(), null), type, value, Clock.Now);
        }
    }
}
