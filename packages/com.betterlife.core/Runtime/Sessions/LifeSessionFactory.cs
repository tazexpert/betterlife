using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
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

        /// <summary>
        /// Creates the session.
        /// </summary>
        /// <param name="content">Loaded content.</param>
        /// <param name="creation">The created life.</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="policy">Interrupt policy, or null for the default.</param>
        /// <param name="bonusSources">Item chance bonus sources, or null.</param>
        /// <param name="extraPhases">Tick phases contributed by other systems, or null.</param>
        /// <returns>The session.</returns>
        public static LifeSession CreateNew(IContentRepository content, CreationResult creation, IEventBus bus,
            InterruptPolicy policy = null, IEnumerable<IChanceBonusSource> bonusSources = null, IEnumerable<ITickPhase> extraPhases = null)
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
            var timeState = new TimeState(clock, budget);
            var eventState = new EventState();
            var luck = new LuckCalculator(content.Get<LuckSettings>(), content.Get<FortuneCalculator>(), content.Get<KarmaService>(), bonusSources);
            EventSystem events = EventSystemFactory.Create(content, eventState, luck, creation.Random, bus, clock);
            var deaths = new DeathService(content.Get<DefinitionCatalog<DeathCauseDefinition>>(), bus);
            var economySettings = content.Get<EconomySettings>();
            var economyRandom = creation.Random.GetStream(EconomyStream);
            var economy = new EconomyService(economySettings,
                EconomyService.CreateInitial(economySettings, creation.Character.Birth.Week.Year, economyRandom), economyRandom);
            var financeSettings = content.Get<FinanceSettings>();
            var tracks = content.Get<DefinitionCatalog<CareerTrackDefinition>>();
            var careerRandom = creation.Random.GetStream(CareerStream);
            var incomeSources = new IIncomeSource[]
            {
                new FamilyAllowanceSource(financeSettings), new CareerIncomeSource(tracks, economy, careerRandom), new PensionIncomeSource(),
            };
            var finance = new FinanceService(creation.Character, content.Get<DefinitionCatalog<AssetDefinition>>(),
                content.Get<DefinitionCatalog<DebtProductDefinition>>(), content.Get<DefinitionCatalog<LifestyleTierDefinition>>(),
                financeSettings, economy, incomeSources, bus, clock);
            var programs = content.Get<DefinitionCatalog<EducationProgramDefinition>>();
            var education = new EducationService(creation.Character, programs, content.Get<EducationSettings>(), settings, budget, luck,
                creation.Random.GetStream(EducationStream), finance, finance, bus, clock);
            var npcs = new NpcFactory(content.Get<GeneticsService>(), content.Get<DefinitionCatalog<NameList>>(), creation.Random.GetStream(NpcStream));
            var relationships = new RelationshipService(creation.Character, content.Get<RelationshipSettings>(),
                content.Get<DefinitionCatalog<InteractionDefinition>>(), npcs, content.Get<MortalityModel>(), luck,
                creation.Random.GetStream(RelationshipStream), finance, bus, clock);
            relationships.InitializeFamily();
            var career = new CareerService(creation.Character, tracks, content.Get<CareerSettings>(), programs, budget, settings, luck,
                careerRandom, economy, bus, clock, relationships);
            IEnumerable<ITickPhase> phases = new ITickPhase[]
            {
                new AgingPhase(timeState, settings, creation.Random.GetStream(HealthStream), bus),
                new HealthConditionPhase(creation.Random.GetStream(HealthStream), bus),
                new EconomyYearPhase(economy),
                new FinanceMonthPhase(finance),
                new EducationWeekPhase(education),
                new CareerWeekPhase(career),
                new CareerMonthPhase(career),
                new RelationshipMonthPhase(relationships),
                new RelationshipYearPhase(relationships),
                new EventRollPhase(events),
                new DeathCheckPhase(content.Get<MortalityModel>(), deaths, creation.Random.GetStream(DeathStream)),
            }.Concat(extraPhases ?? Enumerable.Empty<ITickPhase>());
            TimeSystem time = TimeSystemFactory.Create(content, timeState, creation.Random, bus, policy ?? new InterruptPolicy(), phases);
            return new LifeSession(creation.Character, creation.Random, timeState, eventState, time, events, luck, deaths, economy, finance, education, career, relationships, bus);
        }
    }
}
