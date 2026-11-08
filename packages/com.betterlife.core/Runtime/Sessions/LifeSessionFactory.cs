using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Events;
using BetterLife.Core.Karma;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Luck;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Sessions
{
    /// <summary>
    /// Composes a <see cref="LifeSession"/> for a newly created life, starting at the birth week.
    /// </summary>
    public static class LifeSessionFactory
    {
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
            var timeState = new TimeState(clock, TimeBudget.CreateDefault(content.Get<DefinitionCatalog<ActivityDefinition>>(), content.Get<TimeSettings>()));
            var eventState = new EventState();
            var luck = new LuckCalculator(content.Get<LuckSettings>(), content.Get<FortuneCalculator>(), content.Get<KarmaService>(), bonusSources);
            EventSystem events = EventSystemFactory.Create(content, eventState, luck, creation.Random, bus, clock);
            IEnumerable<ITickPhase> phases = new ITickPhase[] { new EventRollPhase(events) }.Concat(extraPhases ?? Enumerable.Empty<ITickPhase>());
            TimeSystem time = TimeSystemFactory.Create(content, timeState, creation.Random, bus, policy ?? new InterruptPolicy(), phases);
            return new LifeSession(creation.Character, creation.Random, timeState, eventState, time, events, luck, bus);
        }
    }
}
