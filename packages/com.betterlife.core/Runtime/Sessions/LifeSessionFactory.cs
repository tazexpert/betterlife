using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Randomness;
using BetterLife.Core.Persistence;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Careers;
using BetterLife.Core.Economy;
using BetterLife.Core.Education;
using BetterLife.Core.Events;
using BetterLife.Core.Karma;
using BetterLife.Core.Life;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Luck;
using BetterLife.Core.Relationships;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.Vices;

namespace BetterLife.Core.Sessions
{
    /// <summary>
    /// Composes a <see cref="LifeSession"/> for a newly created life, starting at the birth week with the first
    /// life stage's weekly plan.
    /// </summary>
    public static class LifeSessionFactory
    {
        /// <summary>Random stream for aging and illness.</summary>
        public const string HealthStream = "health";

        /// <summary>Random stream for death rolls.</summary>
        public const string DeathStream = "death";

        /// <summary>Random stream for the economy and markets.</summary>
        public const string EconomyStream = "economy";

        /// <summary>Random stream for entrance exams and scholarships.</summary>
        public const string EducationStream = "education";

        /// <summary>Random stream for hiring, promotions, career risks and variable income.</summary>
        public const string CareerStream = "career";

        /// <summary>Random stream for relationships (meeting, interactions, NPC deaths).</summary>
        public const string RelationshipStream = "relationships";

        /// <summary>Random stream for generating NPCs.</summary>
        public const string NpcStream = "relationships.npcs";

        /// <summary>Random stream for vices.</summary>
        public const string ViceStream = "vices";

        /// <summary>
        /// Creates the session for a newly created life.
        /// </summary>
        /// <param name="content">Loaded content.</param>
        /// <param name="creation">The created life.</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="policy">Interrupt policy, or null for the default.</param>
        /// <param name="bonusSources">Item chance bonus sources, or null.</param>
        /// <param name="extraPhases">Tick phases contributed by other systems, or null.</param>
        /// <param name="ironman">True for an Ironman life (no Undo).</param>
        /// <returns>The session.</returns>
        public static LifeSession CreateNew(IContentRepository content, CreationResult creation, IEventBus bus,
            InterruptPolicy policy = null, IEnumerable<IChanceBonusSource> bonusSources = null, IEnumerable<ITickPhase> extraPhases = null,
            bool ironman = false)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (creation == null)
            {
                throw new ArgumentNullException(nameof(creation));
            }

            var clock = new GameClock(creation.Character.Birth.Week);
            var settings = content.Get<TimeSettings>();
            var budget = new TimeBudget(content.Get<DefinitionCatalog<ActivityDefinition>>(), settings);
            budget.Reset(settings.BudgetFor(creation.Character.LifeStage(clock.Now).Id));
            var economySettings = content.Get<EconomySettings>();
            EconomyState economy = EconomyService.CreateInitial(economySettings, creation.Character.Birth.Week.Year, creation.Random.GetStream(EconomyStream));
            LifeSession session = Compose(content, creation.Character, creation.Random, new TimeState(clock, budget), new EventState(), economy, bus,
                policy, bonusSources, extraPhases, creation.Mode, ironman);
            session.Relationships.InitializeFamily();
            return session;
        }

        /// <summary>
        /// Rebuilds a saved life. Content that no longer exists is dropped and reported as warnings.
        /// </summary>
        /// <param name="content">Loaded content.</param>
        /// <param name="save">Saved life (already migrated to the current save version).</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="policy">Interrupt policy, or null for the default.</param>
        /// <param name="bonusSources">Item chance bonus sources, or null.</param>
        /// <param name="extraPhases">Tick phases contributed by other systems, or null.</param>
        /// <returns>The session and any warnings.</returns>
        public static LifeSessionLoadResult Load(IContentRepository content, LifeSaveDto save, IEventBus bus,
            InterruptPolicy policy = null, IEnumerable<IChanceBonusSource> bonusSources = null, IEnumerable<ITickPhase> extraPhases = null)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (save == null || save.Character == null || save.Time == null || save.Events == null || save.Random == null || save.Economy == null)
            {
                throw new FormatException("The save is missing a section.");
            }

            var warnings = new List<string>();
            RandomRegistry random = FoundationDtoMapper.ToRandomRegistry(save.Random);
            CharacterLoadResult character = CharacterDtoMapper.FromDto(save.Character, content.Get<CharacterCatalogs>());
            warnings.AddRange(character.Warnings);
            warnings.AddRange(DropUnknownContent(content, character.Character));
            TimeStateLoadResult time = TimeStateDtoMapper.FromDto(save.Time, content.Get<DefinitionCatalog<ActivityDefinition>>(), content.Get<TimeSettings>());
            warnings.AddRange(time.Warnings);
            EventStateLoadResult events = EventStateDtoMapper.FromDto(save.Events, content.Get<DefinitionCatalog<LifeEventDefinition>>());
            warnings.AddRange(events.Warnings);
            EconomyState economy = EconomyStateDtoMapper.FromDto(save.Economy, content.Get<EconomySettings>());
            CreationMode mode = Enum.TryParse(save.Mode, out CreationMode parsed) && Enum.IsDefined(typeof(CreationMode), parsed) ? parsed : CreationMode.Random;

            LifeSession session = Compose(content, character.Character, random, time.State, events.State, economy, bus, policy, bonusSources, extraPhases,
                mode, save.Ironman);
            session.Relationships.PendingSocialHours = save.PendingSocialHours;
            return new LifeSessionLoadResult(session, warnings);
        }

        private static IEnumerable<string> DropUnknownContent(IContentRepository content, Character character)
        {
            var assets = content.Get<DefinitionCatalog<AssetDefinition>>();
            var products = content.Get<DefinitionCatalog<DebtProductDefinition>>();
            var lifestyles = content.Get<DefinitionCatalog<LifestyleTierDefinition>>();
            var programs = content.Get<DefinitionCatalog<EducationProgramDefinition>>();
            var tracks = content.Get<DefinitionCatalog<CareerTrackDefinition>>();
            var vices = content.Get<DefinitionCatalog<ViceDefinition>>();
            return FinancesDtoMapper.DropUnknown(character.Finances, assets.Contains, products.Contains, lifestyles.Contains)
                .Concat(character.Education.DropUnknown(programs.Contains, (p, i) => programs.Get(p).Institutions.Contains(i)))
                .Concat(character.Career.DropUnknown(id => tracks.TryGet(id, out CareerTrackDefinition track) ? track.Levels.Count : (int?)null))
                .Concat(character.Vices.DropUnknown(vices.Contains))
                .ToList();
        }

        private static LifeSession Compose(IContentRepository content, Character character, RandomRegistry random, TimeState timeState, EventState eventState,
            EconomyState economyState, IEventBus bus, InterruptPolicy policy, IEnumerable<IChanceBonusSource> bonusSources, IEnumerable<ITickPhase> extraPhases,
            CreationMode mode, bool ironman)
        {
            GameClock clock = timeState.Clock;
            TimeBudget budget = timeState.Budget;
            var settings = content.Get<TimeSettings>();
            var luck = new LuckCalculator(content.Get<LuckSettings>(), content.Get<FortuneCalculator>(), content.Get<KarmaService>(), bonusSources);
            EventSystem events = EventSystemFactory.Create(content, eventState, luck, random, bus, clock);
            var deaths = new DeathService(content.Get<DefinitionCatalog<DeathCauseDefinition>>(), bus);
            var economy = new EconomyService(content.Get<EconomySettings>(), economyState, random.GetStream(EconomyStream));
            var financeSettings = content.Get<FinanceSettings>();
            var tracks = content.Get<DefinitionCatalog<CareerTrackDefinition>>();
            var careerRandom = random.GetStream(CareerStream);
            var incomeSources = new IIncomeSource[]
            {
                new FamilyAllowanceSource(financeSettings), new CareerIncomeSource(tracks, economy, careerRandom), new PensionIncomeSource(),
            };
            var finance = new FinanceService(character, content.Get<DefinitionCatalog<AssetDefinition>>(),
                content.Get<DefinitionCatalog<DebtProductDefinition>>(), content.Get<DefinitionCatalog<LifestyleTierDefinition>>(),
                financeSettings, economy, incomeSources, bus, clock);
            var programs = content.Get<DefinitionCatalog<EducationProgramDefinition>>();
            var education = new EducationService(character, programs, content.Get<EducationSettings>(), settings, budget, luck,
                random.GetStream(EducationStream), finance, finance, bus, clock);
            var npcs = new NpcFactory(content.Get<GeneticsService>(), content.Get<DefinitionCatalog<NameList>>(), random.GetStream(NpcStream));
            var relationships = new RelationshipService(character, content.Get<RelationshipSettings>(),
                content.Get<DefinitionCatalog<InteractionDefinition>>(), npcs, content.Get<MortalityModel>(), luck,
                random.GetStream(RelationshipStream), finance, bus, clock);
            var vices = new ViceService(character, content.Get<DefinitionCatalog<ViceDefinition>>(),
                content.Get<DefinitionCatalog<QuitMethodDefinition>>(), content.Get<ViceSettings>(), luck, random.GetStream(ViceStream), bus, clock);
            var career = new CareerService(character, tracks, content.Get<CareerSettings>(), programs, budget, settings, luck,
                careerRandom, economy, bus, clock, relationships);
            IEnumerable<ITickPhase> phases = new ITickPhase[]
            {
                new AgingPhase(timeState, settings, random.GetStream(HealthStream), bus),
                new HealthConditionPhase(random.GetStream(HealthStream), bus),
                new EconomyYearPhase(economy),
                new FinanceMonthPhase(finance),
                new EducationWeekPhase(education),
                new CareerWeekPhase(career),
                new CareerMonthPhase(career),
                new RelationshipMonthPhase(relationships),
                new RelationshipYearPhase(relationships),
                new ViceWeekPhase(vices),
                new EventRollPhase(events),
                new DeathCheckPhase(content.Get<MortalityModel>(), deaths, random.GetStream(DeathStream)),
            }.Concat(extraPhases ?? Enumerable.Empty<ITickPhase>());
            TimeSystem time = TimeSystemFactory.Create(content, timeState, random, bus, policy ?? new InterruptPolicy(), phases);
            return new LifeSession(character, random, timeState, eventState, time, events, luck, deaths, economy, finance, education, career,
                relationships, vices, bus, mode, ironman);
        }
    }
}
