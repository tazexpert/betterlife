using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Events;
using BetterLife.Core.Karma;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking.Phases;

namespace BetterLife.Core.Time.Ticking
{
    /// <summary>
    /// Builds a <see cref="TimeSystem"/> with the standard phases from loaded content. Used by the composition root
    /// and by headless simulations; later systems pass their own phases in <c>extraPhases</c>.
    /// </summary>
    public static class TimeSystemFactory
    {
        /// <summary>Name of the random stream used by the allocation phase.</summary>
        public const string RandomStream = "time";

        /// <summary>
        /// Creates the time system.
        /// </summary>
        /// <param name="content">Loaded content (activities and time settings).</param>
        /// <param name="state">Time state of the life.</param>
        /// <param name="random">Random registry of the life.</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="policy">Interrupt policy.</param>
        /// <param name="extraPhases">Phases contributed by other systems.</param>
        /// <returns>The time system.</returns>
        public static TimeSystem Create(
            IContentRepository content,
            TimeState state,
            RandomRegistry random,
            IEventBus bus,
            InterruptPolicy policy,
            IEnumerable<ITickPhase> extraPhases = null)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            if (random == null)
            {
                throw new ArgumentNullException(nameof(random));
            }

            var phases = new List<ITickPhase>
            {
                new TimeAllocationPhase(state, content.Get<DefinitionCatalog<ActivityDefinition>>(), content.Get<TimeSettings>(), random.GetStream(RandomStream), bus),
                new NaturalDecayPhase(),
                new KarmicDebtPhase(bus),
                new WeeklyRecordPhase(),
            };
            phases.AddRange(extraPhases ?? Enumerable.Empty<ITickPhase>());
            return new TimeSystem(state.Clock, phases, bus, policy);
        }
    }
}
