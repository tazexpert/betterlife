using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Events;

namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// Advances the game week by week. Each week runs every due phase in <see cref="ITickPhase.Order"/>,
    /// then stops early if the interrupt policy says so.
    /// </summary>
    public sealed class TimeSystem
    {
        /// <summary>Interrupt reason used when the character dies during a skip.</summary>
        public const string DeathReasonKey = "interrupt.death";

        private readonly GameClock _clock;
        private readonly IEventBus _bus;
        private readonly List<ITickPhase> _phases;

        /// <summary>
        /// Creates the time system.
        /// </summary>
        /// <param name="clock">Game clock.</param>
        /// <param name="phases">Phases with unique names.</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="policy">Interrupt policy.</param>
        public TimeSystem(GameClock clock, IEnumerable<ITickPhase> phases, IEventBus bus, InterruptPolicy policy)
        {
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            Policy = policy ?? throw new ArgumentNullException(nameof(policy));
            if (phases == null)
            {
                throw new ArgumentNullException(nameof(phases));
            }

            // Stable sort: phases with the same order keep registration order.
            _phases = phases.Select((phase, index) => (phase, index))
                .OrderBy(p => p.phase?.Order ?? 0).ThenBy(p => p.index)
                .Select(p => p.phase).ToList();
            if (_phases.Any(p => p == null))
            {
                throw new ArgumentException("Phases must not contain null.", nameof(phases));
            }
            if (_phases.Select(p => p.Name).Distinct(StringComparer.Ordinal).Count() != _phases.Count)
            {
                throw new ArgumentException("Phase names must be unique.", nameof(phases));
            }
        }

        /// <summary>The game clock.</summary>
        public GameClock Clock => _clock;

        /// <summary>Phases in execution order.</summary>
        public IReadOnlyList<ITickPhase> Phases => _phases;

        /// <summary>Interrupt policy (threshold is a player setting).</summary>
        public InterruptPolicy Policy { get; }

        /// <summary>
        /// Returns how many weeks a skip covers from the current week.
        /// </summary>
        /// <param name="skip">Skip length.</param>
        /// <returns>Weeks, at least 1.</returns>
        public int WeeksFor(TimeSkip skip)
        {
            switch (skip)
            {
                case TimeSkip.Week:
                    return 1;
                case TimeSkip.Month:
                    return WeeksUntil(d => d.IsMonthStart);
                case TimeSkip.Year:
                    return WeeksUntil(d => d.IsYearStart);
                default:
                    throw new ArgumentOutOfRangeException(nameof(skip), skip, "Unknown skip.");
            }
        }

        /// <summary>
        /// Skips time for the player character.
        /// </summary>
        /// <param name="character">The player character.</param>
        /// <param name="skip">Skip length.</param>
        /// <returns>The outcome.</returns>
        public AdvanceResult Advance(Character character, TimeSkip skip)
        {
            return AdvanceWeeks(character, WeeksFor(skip));
        }

        /// <summary>
        /// Simulates up to <paramref name="weeks"/> weeks, stopping after a week whose interrupts meet the policy
        /// or in which the character dies. Does nothing for a dead character.
        /// </summary>
        /// <param name="character">The player character.</param>
        /// <param name="weeks">Positive number of weeks.</param>
        /// <returns>The outcome.</returns>
        public AdvanceResult AdvanceWeeks(Character character, int weeks)
        {
            if (character == null)
            {
                throw new ArgumentNullException(nameof(character));
            }
            if (weeks <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(weeks), weeks, "Weeks must be positive.");
            }

            GameDate from = _clock.Now;
            var interrupts = new List<InterruptRequest>();
            InterruptRequest stoppedBy = null;
            for (int i = 0; i < weeks && stoppedBy == null && character.IsAlive; i++)
            {
                TickContext context = RunWeek(character);
                interrupts.AddRange(context.Interrupts);
                InterruptRequest stop = Policy.FindStop(context.Interrupts);
                if (!character.IsAlive)
                {
                    // A death always ends the skip, even in its last week or when caused by an event effect.
                    stoppedBy = stop ?? new InterruptRequest(Severity.Critical, DeathReasonKey, character.Death.CauseId, context.Week);
                }
                else if (stop != null && i < weeks - 1)
                {
                    stoppedBy = stop;
                }
            }

            var result = new AdvanceResult(from, _clock.Now, weeks, interrupts, stoppedBy);
            _bus.Publish(new TimeAdvancedEvent(result));
            return result;
        }

        private TickContext RunWeek(Character character)
        {
            var context = new TickContext(_clock.NextWeek, character);
            foreach (ITickPhase phase in _phases.Where(p => context.Runs(p.Cadence)))
            {
                phase.Execute(context);
            }
            _clock.MoveTo(context.Week);
            _bus.Publish(new WeekCompletedEvent(context.Week));
            return context;
        }

        private int WeeksUntil(Func<GameDate, bool> isTarget)
        {
            int weeks = 1;
            while (!isTarget(_clock.Now.AddWeeks(weeks)))
            {
                weeks++;
            }
            return weeks;
        }
    }
}
