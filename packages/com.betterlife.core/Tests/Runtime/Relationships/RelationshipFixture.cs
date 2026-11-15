using System.Collections.Generic;
using BetterLife.Core.Astrology;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Karma;
using BetterLife.Core.Life;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Relationships;
using BetterLife.Core.Tests.Careers;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.Time.Ticking.Phases;

namespace BetterLife.Core.Tests.Relationships
{
    /// <summary>
    /// A character born in mid-2000 (male, Thai) with warm and strict parents and a relationship service from the test content.
    /// </summary>
    internal sealed class RelationshipFixture
    {
        public RelationshipFixture(int year = 2030, ulong seed = 5UL, bool family = true)
        {
            IContentRepository content = SchoolWorkFixture.Content;
            Character = TestCatalogs.NewCharacter();
            if (family)
            {
                Character.SetFamily(new FamilyInfo("poor", new[]
                {
                    new ParentRecord(new PersonName("Malee", "Jaidee"), BiologicalSex.Female, 1972, "warm", new Genome(), new[] { "calm" }),
                    new ParentRecord(new PersonName("Somsak", "Jaidee"), BiologicalSex.Male, 1970, "strict", new Genome(), new string[0]),
                }));
            }
            Clock = new GameClock(GameDate.FromYearWeek(year, 0));
            Bus = new EventBus();
            Settings = content.Get<RelationshipSettings>();
            var registry = new RandomRegistry(seed);
            Factory = new NpcFactory(content.Get<GeneticsService>(), content.Get<DefinitionCatalog<NameList>>(), registry.GetStream("npcs"));
            var luck = new LuckCalculator(content.Get<LuckSettings>(), content.Get<FortuneCalculator>(), content.Get<KarmaService>());
            Service = new RelationshipService(Character, Settings, content.Get<DefinitionCatalog<InteractionDefinition>>(), Factory,
                content.Get<MortalityModel>(), luck, registry.GetStream("relationships"), Dependency, Bus, Clock);
        }

        public Character Character { get; }

        public CharacterRelationships People => Character.Relationships;

        public GameClock Clock { get; }

        public EventBus Bus { get; }

        public RelationshipSettings Settings { get; }

        public NpcFactory Factory { get; }

        public FakeDependency Dependency { get; } = new FakeDependency { Independent = true };

        public RelationshipService Service { get; }

        public TickContext LastTick { get; private set; }

        /// <summary>Adds an NPC of a type and value; age in years at the current week.</summary>
        public Relationship Add(RelationshipType type, float value, int age = 30, BiologicalSex sex = BiologicalSex.Female, params string[] traits)
        {
            GameDate born = Clock.Now.AddWeeks(-age * GameCalendar.WeeksPerYear);
            return People.Add(id => new Npc(id, new PersonName("Npc" + id, "Test"), sex, born, new Genome(), traits), type, value, Clock.Now);
        }

        /// <summary>Spends social hours over a month, runs the monthly update and moves four weeks on.</summary>
        public void Month(int socialHours = 0)
        {
            Bus.Publish(new TimeAllocationAppliedEvent(Clock.Now, new Dictionary<string, int> { ["socialize"] = socialHours }, new StatChangeResult[0], 0m));
            LastTick = new TickContext(Clock.Now, Character);
            Service.ProcessMonth(LastTick);
            Clock.MoveTo(Clock.Now.AddWeeks(4));
        }

        /// <summary>Runs the yearly update at the current week.</summary>
        public void Year()
        {
            LastTick = new TickContext(Clock.Now, Character);
            Service.ProcessYear(LastTick);
        }
    }
}
