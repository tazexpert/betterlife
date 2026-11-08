using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Events;
using BetterLife.Core.Karma;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// Runs life events: weekly random draws, scheduled consequences, karmic misfortune, the inbox and answering choices.
    /// Draws use one random stream and choice rolls another, so player decisions never change which events come later.
    /// </summary>
    public sealed class EventSystem : IDisposable
    {
        /// <summary>Source key for rule evaluation that is not tied to a specific event.</summary>
        public const string SourceKey = "source.events";

        private readonly DefinitionCatalog<LifeEventDefinition> _events;
        private readonly EventSettings _settings;
        private readonly LuckCalculator _luck;
        private readonly IRandomProvider _rollRandom;
        private readonly IRandomProvider _choiceRandom;
        private readonly IEventBus _bus;
        private readonly GameClock _clock;
        private readonly List<KarmicDebt> _dueDebts = new List<KarmicDebt>();
        private readonly List<IDisposable> _subscriptions = new List<IDisposable>();

        /// <summary>
        /// Creates the system and subscribes to scheduling requests and karmic debts.
        /// </summary>
        /// <param name="events">Event catalog.</param>
        /// <param name="settings">Event settings.</param>
        /// <param name="state">Event state of the life.</param>
        /// <param name="luck">Chance calculator.</param>
        /// <param name="rollRandom">Stream for drawing events.</param>
        /// <param name="choiceRandom">Stream for chance rolls and effect ranges.</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="clock">Game clock (the current week for answers).</param>
        public EventSystem(DefinitionCatalog<LifeEventDefinition> events, EventSettings settings, EventState state, LuckCalculator luck,
            IRandomProvider rollRandom, IRandomProvider choiceRandom, IEventBus bus, GameClock clock)
        {
            _events = events ?? throw new ArgumentNullException(nameof(events));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            State = state ?? throw new ArgumentNullException(nameof(state));
            _luck = luck ?? throw new ArgumentNullException(nameof(luck));
            _rollRandom = rollRandom ?? throw new ArgumentNullException(nameof(rollRandom));
            _choiceRandom = choiceRandom ?? throw new ArgumentNullException(nameof(choiceRandom));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));

            _subscriptions.Add(_bus.Subscribe<ScheduleEventRequest>(r => State.Schedule(new ScheduledEvent(r.EventId, r.DueOn))));
            _subscriptions.Add(_bus.Subscribe<KarmicDebtDueEvent>(e => _dueDebts.Add(e.Debt)));
        }

        /// <summary>Event state of the life.</summary>
        public EventState State { get; }

        /// <summary>Events waiting for a choice.</summary>
        public IReadOnlyList<PendingEvent> Inbox => State.Inbox;

        /// <summary>
        /// Runs this week's events: overdue answers, scheduled events, karmic misfortune, then the random draw.
        /// </summary>
        /// <param name="tick">Tick context.</param>
        public void RunWeek(TickContext tick)
        {
            if (tick == null)
            {
                throw new ArgumentNullException(nameof(tick));
            }

            Character character = tick.Character;
            if (!character.IsAlive)
            {
                return;
            }
            foreach (PendingEvent overdue in State.Inbox.Where(p => p.Deadline <= tick.Week).ToList())
            {
                AutoResolve(character, overdue, tick.Week);
            }

            var context = new RuleContext(character, tick.Week, _rollRandom, SourceKey);
            foreach (ScheduledEvent scheduled in State.TakeDue(tick.Week))
            {
                if (_events.TryGet(scheduled.EventId, out LifeEventDefinition definition) && definition.IsEligible(context) && UnderLimit(definition))
                {
                    Fire(definition, EventOrigin.Scheduled, tick);
                }
            }

            foreach (KarmicDebt debt in _dueDebts.ToList())
            {
                LifeEventDefinition misfortune = Draw(EventTrigger.KarmicDebt, context, e => !e.DebtAmount.HasValue || InRange(e.DebtAmount.Value, debt.Amount));
                if (misfortune != null)
                {
                    Fire(misfortune, EventOrigin.KarmicDebt, tick);
                }
            }
            _dueDebts.Clear();

            if (_rollRandom.Chance(_settings.WeeklyEventChance))
            {
                LifeEventDefinition drawn = Draw(EventTrigger.Random, context, e => true);
                if (drawn != null)
                {
                    Fire(drawn, EventOrigin.Random, tick);
                }
            }
        }

        /// <summary>
        /// Describes every choice of a pending event for the dialog.
        /// </summary>
        /// <param name="character">The player character.</param>
        /// <param name="instanceId">Occurrence id.</param>
        /// <returns>One view per choice, in order.</returns>
        public IReadOnlyList<ChoiceView> GetChoices(Character character, int instanceId)
        {
            PendingEvent pending = RequirePending(instanceId);
            LifeEventDefinition definition = _events.Get(pending.EventId);
            RuleContext context = ChoiceContext(character, definition, _clock.Now);
            return definition.Choices.Select(choice =>
            {
                bool available = choice.IsAvailable(context);
                ConditionTrace requirement = choice.Condition?.Explain(context);
                ChanceBreakdown chance = choice.Chance == null ? null : _luck.Explain(choice.Chance, character, _clock.Now);
                IReadOnlyList<RuleText> failure = choice.Failure == null ? new RuleText[0] : choice.Failure.Effects.Preview(context);
                return new ChoiceView(choice, available, available || !choice.HideWhenLocked, requirement, chance, choice.Success.Effects.Preview(context), failure);
            }).ToList();
        }

        /// <summary>
        /// Answers a pending event with the player's choice.
        /// </summary>
        /// <param name="character">The player character.</param>
        /// <param name="instanceId">Occurrence id.</param>
        /// <param name="choiceId">Chosen option; must be available.</param>
        /// <returns>The resolution.</returns>
        public EventResolution Resolve(Character character, int instanceId, string choiceId)
        {
            PendingEvent pending = RequirePending(instanceId);
            LifeEventDefinition definition = _events.Get(pending.EventId);
            EventChoice choice = definition.FindChoice(choiceId) ?? throw new ArgumentException($"Event '{definition.Id}' has no choice '{choiceId}'.", nameof(choiceId));
            if (!choice.IsAvailable(ChoiceContext(character, definition, _clock.Now)))
            {
                throw new InvalidOperationException($"Choice '{choiceId}' is locked.");
            }
            return Apply(character, pending, definition, choice, _clock.Now, false);
        }

        /// <summary>
        /// Unsubscribes from the event bus.
        /// </summary>
        public void Dispose()
        {
            foreach (IDisposable subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
        }

        private void AutoResolve(Character character, PendingEvent pending, GameDate week)
        {
            if (!_events.TryGet(pending.EventId, out LifeEventDefinition definition))
            {
                State.RemovePending(pending);
                return;
            }

            RuleContext context = ChoiceContext(character, definition, week);
            EventChoice preferred = definition.FindChoice(definition.DefaultChoiceId);
            EventChoice choice = preferred != null && preferred.IsAvailable(context)
                ? preferred
                : definition.Choices.FirstOrDefault(c => c.IsAvailable(context));
            Apply(character, pending, definition, choice, week, true);
        }

        private EventResolution Apply(Character character, PendingEvent pending, LifeEventDefinition definition, EventChoice choice, GameDate week, bool automatic)
        {
            ChanceRoll roll = null;
            EventOutcome outcome = null;
            EffectResult result = EffectResult.Empty;
            if (choice != null)
            {
                roll = choice.Chance == null ? null : _luck.Roll(choice.Chance, character, week, _choiceRandom);
                outcome = roll == null || roll.Success ? choice.Success : choice.Failure;
                result = outcome.Effects.Apply(ChoiceContext(character, definition, week));
            }

            State.RemovePending(pending);
            var resolution = new EventResolution(pending, definition, choice, roll, outcome, result, week, automatic);
            State.AddHistory(new EventHistoryEntry(pending.InstanceId, definition.Id, pending.Origin, week, choice?.Id, resolution.Succeeded));
            result.PublishTo(_bus);
            _bus.Publish(new LifeEventResolvedEvent(resolution));
            return resolution;
        }

        private void Fire(LifeEventDefinition definition, EventOrigin origin, TickContext tick)
        {
            int instanceId = State.NewInstanceId();
            State.RecordFire(definition.Id, definition.Tone, tick.Week);
            EffectResult onFire = definition.OnFire.Apply(ChoiceContext(tick.Character, definition, tick.Week));

            if (definition.HasChoices)
            {
                State.AddPending(new PendingEvent(instanceId, definition.Id, origin, tick.Week, tick.Week.AddWeeks(_settings.ResponseWindowWeeks)));
            }
            else
            {
                State.AddHistory(new EventHistoryEntry(instanceId, definition.Id, origin, tick.Week, null, true));
            }

            tick.RequestInterrupt(definition.Severity, definition.TitleKey, definition.Id);
            onFire.PublishTo(_bus);
            _bus.Publish(new LifeEventFiredEvent(instanceId, definition, origin, tick.Week, onFire));
        }

        private LifeEventDefinition Draw(EventTrigger trigger, RuleContext context, Func<LifeEventDefinition, bool> extraFilter)
        {
            bool softenBadNews = NegativeStreakActive(context.Now);
            var candidates = new List<(LifeEventDefinition Event, float Weight)>();
            foreach (LifeEventDefinition definition in _events.All)
            {
                if (definition.Trigger != trigger || !extraFilter(definition) || !UnderLimit(definition) || OnCooldown(definition, context.Now) || !definition.IsEligible(context))
                {
                    continue;
                }
                float weight = definition.Weight.Evaluate(context);
                if (softenBadNews && definition.Tone == EventTone.Negative)
                {
                    weight *= _settings.AntiFrustration.NegativeWeightMultiplier;
                }
                if (weight > 0f)
                {
                    candidates.Add((definition, weight));
                }
            }
            if (candidates.Count == 0)
            {
                return null;
            }

            float roll = _rollRandom.NextFloat() * candidates.Sum(c => c.Weight);
            foreach ((LifeEventDefinition definition, float weight) in candidates)
            {
                roll -= weight;
                if (roll < 0f)
                {
                    return definition;
                }
            }
            return candidates[candidates.Count - 1].Event;
        }

        private bool NegativeStreakActive(GameDate now)
        {
            AntiFrustrationSettings settings = _settings.AntiFrustration;
            int streak = 0;
            for (int i = State.Tones.Count - 1; i >= 0; i--)
            {
                ToneEntry entry = State.Tones[i];
                if (entry.Week.WeeksUntil(now) >= settings.WindowWeeks || entry.Tone != EventTone.Negative)
                {
                    break;
                }
                streak++;
            }
            return streak >= settings.StreakThreshold;
        }

        private bool UnderLimit(LifeEventDefinition definition)
        {
            EventRecord record = State.FindRecord(definition.Id);
            return !definition.MaxOccurrences.HasValue || record == null || record.Occurrences < definition.MaxOccurrences.Value;
        }

        private bool OnCooldown(LifeEventDefinition definition, GameDate now)
        {
            EventRecord record = State.FindRecord(definition.Id);
            return record != null && definition.CooldownWeeks > 0 && record.LastFiredOn.WeeksUntil(now) < definition.CooldownWeeks;
        }

        private RuleContext ChoiceContext(Character character, LifeEventDefinition definition, GameDate week)
        {
            return new RuleContext(character, week, _choiceRandom, definition.TitleKey);
        }

        private PendingEvent RequirePending(int instanceId)
        {
            return State.FindPending(instanceId) ?? throw new ArgumentException($"No pending event with id {instanceId}.", nameof(instanceId));
        }

        private static bool InRange(Rules.FloatRange range, float value) => value >= range.Min && value <= range.Max;
    }
}
