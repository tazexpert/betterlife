using System;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Events;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Builds an <see cref="EventSystem"/> from loaded content for one life.
    /// </summary>
    public static class EventSystemFactory
    {
        /// <summary>Random stream used to draw events.</summary>
        public const string RollStream = "events.roll";

        /// <summary>Random stream used for chance rolls and effect ranges.</summary>
        public const string ChoiceStream = "events.choice";

        /// <summary>
        /// Creates the event system.
        /// </summary>
        /// <param name="content">Loaded content.</param>
        /// <param name="state">Event state of the life.</param>
        /// <param name="luck">Chance calculator.</param>
        /// <param name="random">Random registry of the life.</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="clock">Game clock.</param>
        /// <returns>The event system.</returns>
        public static EventSystem Create(IContentRepository content, EventState state, LuckCalculator luck, RandomRegistry random, IEventBus bus, GameClock clock)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }
            return new EventSystem(
                content.Get<DefinitionCatalog<LifeEventDefinition>>(),
                content.Get<EventSettings>(),
                state,
                luck,
                random.GetStream(RollStream),
                random.GetStream(ChoiceStream),
                bus,
                clock);
        }
    }
}
