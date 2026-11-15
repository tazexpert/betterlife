using System;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Life;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Birthday step (order 50, before time allocation): applies the stage's yearly changes, rolls age-related
    /// conditions, and on entering a new stage resets the weekly plan to that stage's default.
    /// </summary>
    public sealed class AgingPhase : ITickPhase
    {
        /// <summary>Order: before everything else in the week.</summary>
        public const int PhaseOrder = TickOrder.TimeAllocation / 2;

        /// <summary>Interrupt reason when a new stage begins.</summary>
        public const string StageInterruptKey = "interrupt.life_stage";

        private const float HealthNeutral = StatBounds.Max / 2f;
        private const float MinSeverity = 30f;
        private const float MaxSeverity = 70f;

        private readonly TimeState _time;
        private readonly TimeSettings _settings;
        private readonly IRandomProvider _random;
        private readonly IEventBus _bus;

        /// <summary>
        /// Creates the phase.
        /// </summary>
        /// <param name="time">Time state holding the weekly plan.</param>
        /// <param name="settings">Time settings with stage budgets.</param>
        /// <param name="random">Random stream for condition risks.</param>
        /// <param name="bus">Event bus.</param>
        public AgingPhase(TimeState time, TimeSettings settings, IRandomProvider random, IEventBus bus)
        {
            _time = time ?? throw new ArgumentNullException(nameof(time));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
        }

        /// <inheritdoc />
        public string Name => "aging";

        /// <inheritdoc />
        public int Order => PhaseOrder;

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
            GameDate birthWeek = character.Birth.Week;
            if (!character.IsAlive || context.Week <= birthWeek || context.Week.WeekOfYear != birthWeek.WeekOfYear)
            {
                return;
            }

            int age = character.AgeAt(context.Week);
            LifeStageCatalog stages = character.Catalogs.LifeStages;
            LifeStageDefinition previous = stages.StageFor(age - 1);
            LifeStageDefinition stage = stages.StageFor(age);
            _bus.Publish(new BirthdayEvent(character, age, context.Week));

            foreach (StatAmount amount in stage.BirthdayStats)
            {
                character.ApplyStatChange(new StatChange(amount.StatId, amount.Amount, stage.NameKey), context.Week);
            }
            RollConditions(character, stage, context.Week);

            if (stage != previous)
            {
                _time.Budget.Reset(_settings.BudgetFor(stage.Id));
                _bus.Publish(new LifeStageChangedEvent(character, previous, stage, context.Week));
                context.RequestInterrupt(Domain.Severity.Normal, StageInterruptKey, stage.Id);
            }
        }

        private void RollConditions(Character character, LifeStageDefinition stage, GameDate week)
        {
            // Worse health raises the risk: Health 50 = ×1, Health 0 = ×2, Health 100 = ×0.
            float healthFactor = (StatBounds.Max - character.Stats.GetValue(StatIds.Health)) / HealthNeutral;
            foreach (ConditionRisk risk in stage.ConditionRisks)
            {
                bool hit = _random.Chance(risk.AnnualChance * healthFactor);
                float severity = MinSeverity + _random.NextFloat() * (MaxSeverity - MinSeverity);
                if (!hit || character.HasCondition(risk.ConditionId))
                {
                    continue;
                }
                var condition = new HealthCondition(risk.ConditionId, severity, week);
                character.SetCondition(condition);
                _bus.Publish(new HealthConditionStartedEvent(character, condition));
            }
        }
    }
}
