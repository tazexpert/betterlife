using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;
using BetterLife.Core.Time.Budget;

namespace BetterLife.Core.Time.Ticking.Phases
{
    /// <summary>
    /// Applies the weekly plan: stat effects per hour, skill practice scaled by the Mental learning multiplier,
    /// and activity costs. Unpaid costs are published as <see cref="CashShortfallEvent"/>.
    /// </summary>
    public sealed class TimeAllocationPhase : ITickPhase
    {
        /// <summary>Source key for rule evaluation done by this phase.</summary>
        public const string SourceKey = "source.time.allocation";

        private readonly TimeState _state;
        private readonly DefinitionCatalog<ActivityDefinition> _activities;
        private readonly TimeSettings _settings;
        private readonly IRandomProvider _random;
        private readonly IEventBus _bus;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="state">Time state holding the plan.</param>
        /// <param name="activities">Activity catalog.</param>
        /// <param name="settings">Time settings.</param>
        /// <param name="random">Random stream for rule evaluation.</param>
        /// <param name="bus">Event bus.</param>
        public TimeAllocationPhase(TimeState state, DefinitionCatalog<ActivityDefinition> activities, TimeSettings settings, IRandomProvider random, IEventBus bus)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _activities = activities ?? throw new ArgumentNullException(nameof(activities));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc />
        public string Name => "time_allocation";

        /// <inheritdoc />
        public int Order => TickOrder.TimeAllocation;

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
            var rules = new RuleContext(character, context.Week, _random, SourceKey);
            IReadOnlyDictionary<string, int> allocation = _state.Budget.EffectiveAllocation(rules);

            // Learning speed uses Mental at the start of the week so activity order does not matter.
            float learning = _settings.LearningMultiplier(character.Stats.GetValue(StatIds.Mental));
            var changes = new List<StatChangeResult>();
            decimal spent = 0m;
            foreach (ActivityDefinition activity in _activities.All)
            {
                if (!allocation.TryGetValue(activity.Id, out int hours) || hours <= 0)
                {
                    continue;
                }

                foreach (ActivityStatEffect effect in activity.Stats)
                {
                    float delta = effect.TotalFor(hours);
                    if (delta != 0f)
                    {
                        changes.Add(character.ApplyStatChange(new StatChange(effect.StatId, delta, activity.NameKey), context.Week));
                    }
                }
                foreach (ActivitySkillEffect effect in activity.Skills)
                {
                    character.Skills.Practice(effect.SkillId, effect.TotalFor(hours) * learning, context.Week);
                }
                spent += Pay(character, activity, hours, context.Week);
            }

            _bus.Publish(new TimeAllocationAppliedEvent(context.Week, allocation, changes, spent));
        }

        private decimal Pay(Character character, ActivityDefinition activity, int hours, GameDate week)
        {
            decimal cost = activity.CostPerHour * hours;
            if (cost <= 0m)
            {
                return 0m;
            }

            decimal paid = character.Wallet.WithdrawUpTo(cost);
            if (paid < cost)
            {
                _bus.Publish(new CashShortfallEvent(cost - paid, activity.NameKey, week));
            }
            return paid;
        }
    }
}
