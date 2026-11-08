using System;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Weekly effects of health conditions (scaled by severity) and natural recovery (order 210, after decay).
    /// </summary>
    public sealed class HealthConditionPhase : ITickPhase
    {
        private const float SeverityScale = StatBounds.Max;

        private readonly IRandomProvider _random;
        private readonly IEventBus _bus;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="random">Random stream for recovery rolls.</param>
        /// <param name="bus">Event bus.</param>
        public HealthConditionPhase(IRandomProvider random, IEventBus bus)
        {
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc />
        public string Name => "health_conditions";

        /// <inheritdoc />
        public int Order => TickOrder.NaturalDecay + 10;

        /// <inheritdoc />
        public TickCadence Cadence => TickCadence.Weekly;

        /// <inheritdoc />
        public void Execute(TickContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Character character = context.Character;
            if (!character.IsAlive)
            {
                return;
            }
            foreach (HealthCondition condition in character.Conditions.ToList())
            {
                HealthConditionDefinition definition = character.Catalogs.HealthConditions.Get(condition.ConditionId);
                foreach (StatAmount amount in definition.WeeklyStats)
                {
                    character.ApplyStatChange(new StatChange(amount.StatId, amount.Amount * condition.Severity / SeverityScale, definition.NameKey), context.Week);
                }
                if (definition.RecoveryChancePerWeek.HasValue && _random.Chance(definition.RecoveryChancePerWeek.Value))
                {
                    character.RemoveCondition(condition.ConditionId);
                    _bus.Publish(new HealthConditionRecoveredEvent(character, definition, context.Week));
                }
            }
        }
    }
}
