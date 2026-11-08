using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Rules;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// A life event loaded from content/events/*.json.
    /// </summary>
    public sealed class LifeEventDefinition : IDefinition
    {
        /// <summary>
        /// Creates an event.
        /// </summary>
        /// <param name="id">Event id.</param>
        /// <param name="titleKey">Localization key of the title (also the source of its stat changes).</param>
        /// <param name="bodyKey">Localization key of the body.</param>
        /// <param name="category">Category id.</param>
        /// <param name="trigger">How the event occurs.</param>
        /// <param name="severity">Importance (Major and above interrupt time skips by default).</param>
        /// <param name="tone">Good or bad news.</param>
        /// <param name="storyline">Storyline id for grouping, or null.</param>
        /// <param name="condition">Eligibility requirement, or null.</param>
        /// <param name="weight">Weight in its pool.</param>
        /// <param name="cooldownWeeks">Weeks before it may occur again; non-negative.</param>
        /// <param name="maxOccurrences">Lifetime limit, or null.</param>
        /// <param name="debtAmount">Karmic debt sizes it answers (karmic events only), or null.</param>
        /// <param name="onFire">Effects applied when it occurs.</param>
        /// <param name="choices">Options; empty for an announcement.</param>
        /// <param name="defaultChoiceId">Choice used when the player does not respond; required when there are choices.</param>
        public LifeEventDefinition(
            string id,
            string titleKey,
            string bodyKey,
            string category,
            EventTrigger trigger,
            Severity severity,
            EventTone tone,
            string storyline,
            ICondition condition,
            EventWeight weight,
            int cooldownWeeks,
            int? maxOccurrences,
            FloatRange? debtAmount,
            EffectSequence onFire,
            IEnumerable<EventChoice> choices,
            string defaultChoiceId)
        {
            if (cooldownWeeks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldownWeeks), cooldownWeeks, "Cooldown must be non-negative.");
            }
            if (maxOccurrences.HasValue && maxOccurrences.Value < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(maxOccurrences), maxOccurrences, "Max occurrences must be at least 1.");
            }
            if (debtAmount.HasValue && trigger != EventTrigger.KarmicDebt)
            {
                throw new ArgumentException("Only karmic debt events take a debt amount range.", nameof(debtAmount));
            }

            Id = ContentId.Require(id, nameof(id));
            TitleKey = ContentId.RequireLocKey(titleKey, nameof(titleKey));
            BodyKey = ContentId.RequireLocKey(bodyKey, nameof(bodyKey));
            Category = ContentId.Require(category, nameof(category));
            Trigger = trigger;
            Severity = severity;
            Tone = tone;
            Storyline = storyline == null ? null : ContentId.Require(storyline, nameof(storyline));
            Condition = condition;
            Weight = weight ?? throw new ArgumentNullException(nameof(weight));
            CooldownWeeks = cooldownWeeks;
            MaxOccurrences = maxOccurrences;
            DebtAmount = debtAmount;
            OnFire = onFire ?? EffectSequence.Empty;
            Choices = (choices ?? Enumerable.Empty<EventChoice>()).ToList();

            if (Choices.Select(c => c.Id).Distinct(StringComparer.Ordinal).Count() != Choices.Count)
            {
                throw new ArgumentException("Choice ids must be unique within an event.", nameof(choices));
            }
            if (Choices.Count == 0 && defaultChoiceId != null)
            {
                throw new ArgumentException("An event without choices cannot have a default choice.", nameof(defaultChoiceId));
            }
            if (Choices.Count > 0 && Choices.All(c => c.Id != defaultChoiceId))
            {
                throw new ArgumentException($"Default choice '{defaultChoiceId}' is not one of the choices.", nameof(defaultChoiceId));
            }
            DefaultChoiceId = defaultChoiceId;
        }

        /// <inheritdoc />
        public string Id { get; }

        /// <summary>Localization key of the title.</summary>
        public string TitleKey { get; }

        /// <summary>Localization key of the body.</summary>
        public string BodyKey { get; }

        /// <summary>Category id.</summary>
        public string Category { get; }

        /// <summary>How the event occurs.</summary>
        public EventTrigger Trigger { get; }

        /// <summary>Importance.</summary>
        public Severity Severity { get; }

        /// <summary>Good or bad news.</summary>
        public EventTone Tone { get; }

        /// <summary>Storyline id, or null.</summary>
        public string Storyline { get; }

        /// <summary>Eligibility requirement, or null.</summary>
        public ICondition Condition { get; }

        /// <summary>Weight in its pool.</summary>
        public EventWeight Weight { get; }

        /// <summary>Weeks before it may occur again.</summary>
        public int CooldownWeeks { get; }

        /// <summary>Lifetime limit, or null.</summary>
        public int? MaxOccurrences { get; }

        /// <summary>Karmic debt sizes it answers, or null for any.</summary>
        public FloatRange? DebtAmount { get; }

        /// <summary>Effects applied when it occurs.</summary>
        public EffectSequence OnFire { get; }

        /// <summary>Options.</summary>
        public IReadOnlyList<EventChoice> Choices { get; }

        /// <summary>Choice used when the player does not respond, or null.</summary>
        public string DefaultChoiceId { get; }

        /// <summary>True when the player must choose.</summary>
        public bool HasChoices => Choices.Count > 0;

        /// <summary>
        /// Returns a choice by id, or null.
        /// </summary>
        /// <param name="choiceId">Choice id.</param>
        /// <returns>The choice or null.</returns>
        public EventChoice FindChoice(string choiceId) => Choices.FirstOrDefault(c => c.Id == choiceId);

        /// <summary>
        /// Returns true when the eligibility requirement holds.
        /// </summary>
        /// <param name="context">Rule context.</param>
        /// <returns>True when eligible.</returns>
        public bool IsEligible(RuleContext context) => Condition == null || Condition.Evaluate(context);
    }
}
