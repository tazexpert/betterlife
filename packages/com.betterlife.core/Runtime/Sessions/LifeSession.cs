using System;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Events;
using BetterLife.Core.Life;
using BetterLife.Core.LifeEvents;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Sessions
{
    /// <summary>
    /// One life in play: the character, its random registry, time and event state, and the systems that run them.
    /// </summary>
    public sealed class LifeSession : IDisposable
    {
        /// <summary>
        /// Creates a session from already composed parts (use <see cref="LifeSessionFactory"/>).
        /// </summary>
        /// <param name="character">The player character.</param>
        /// <param name="random">Random registry of the life.</param>
        /// <param name="timeState">Time state.</param>
        /// <param name="eventState">Event state.</param>
        /// <param name="time">Time system.</param>
        /// <param name="events">Event system.</param>
        /// <param name="luck">Chance calculator.</param>
        /// <param name="deaths">Death service.</param>
        /// <param name="bus">Event bus.</param>
        public LifeSession(Character character, RandomRegistry random, TimeState timeState, EventState eventState,
            TimeSystem time, EventSystem events, LuckCalculator luck, DeathService deaths, IEventBus bus)
        {
            Character = character ?? throw new ArgumentNullException(nameof(character));
            Random = random ?? throw new ArgumentNullException(nameof(random));
            TimeState = timeState ?? throw new ArgumentNullException(nameof(timeState));
            EventState = eventState ?? throw new ArgumentNullException(nameof(eventState));
            Time = time ?? throw new ArgumentNullException(nameof(time));
            Events = events ?? throw new ArgumentNullException(nameof(events));
            Luck = luck ?? throw new ArgumentNullException(nameof(luck));
            Deaths = deaths ?? throw new ArgumentNullException(nameof(deaths));
            Bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <summary>The player character.</summary>
        public Character Character { get; }

        /// <summary>Random registry of the life.</summary>
        public RandomRegistry Random { get; }

        /// <summary>Life seed.</summary>
        public ulong Seed => Random.LifeSeed;

        /// <summary>Time state (clock and weekly plan).</summary>
        public TimeState TimeState { get; }

        /// <summary>Event state.</summary>
        public EventState EventState { get; }

        /// <summary>Time system.</summary>
        public TimeSystem Time { get; }

        /// <summary>Event system.</summary>
        public EventSystem Events { get; }

        /// <summary>Chance calculator.</summary>
        public LuckCalculator Luck { get; }

        /// <summary>Death service.</summary>
        public DeathService Deaths { get; }

        /// <summary>Event bus.</summary>
        public IEventBus Bus { get; }

        /// <summary>True once the character has died.</summary>
        public bool IsOver => !Character.IsAlive;

        /// <summary>
        /// Skips time.
        /// </summary>
        /// <param name="skip">Skip length.</param>
        /// <returns>The outcome.</returns>
        public AdvanceResult Advance(TimeSkip skip) => Time.Advance(Character, skip);

        /// <summary>
        /// Releases event bus subscriptions.
        /// </summary>
        public void Dispose()
        {
            Events.Dispose();
            Deaths.Dispose();
        }
    }
}
