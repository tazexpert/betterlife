using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Events;
using BetterLife.Core.Karma;
using BetterLife.Core.Life;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Luck;
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
            IEnumerable<ITickPhase> phases = new ITickPhase[]
            {
                new AgingPhase(timeState, settings, creation.Random.GetStream(HealthStream), bus),
                new HealthConditionPhase(creation.Random.GetStream(HealthStream), bus),
                new EventRollPhase(events),
                new DeathCheckPhase(content.Get<MortalityModel>(), deaths, creation.Random.GetStream(DeathStream)),
            }.Concat(extraPhases ?? Enumerable.Empty<ITickPhase>());
            TimeSystem time = TimeSystemFactory.Create(content, timeState, creation.Random, bus, policy ?? new InterruptPolicy(), phases);
            return new LifeSession(creation.Character, creation.Random, timeState, eventState, time, events, luck, deaths, bus);
        }
    }
}
