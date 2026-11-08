using System;
using BetterLife.Core.Domain;
using BetterLife.Core.Luck;
using BetterLife.Core.Rules;

namespace BetterLife.Core.LifeEvents
{
    /// <summary>
    /// One option of an event. With a chance the outcome is rolled; without one it always succeeds.
    /// </summary>
    public sealed class EventChoice
    {
        /// <summary>
        /// Creates a choice.
        /// </summary>
        /// <param name="id">Choice id, unique within the event.</param>
        /// <param name="textKey">Localization key of the button text.</param>
        /// <param name="condition">Requirement to pick the choice, or null.</param>
        /// <param name="hideWhenLocked">True to hide the choice instead of showing it locked.</param>
        /// <param name="chance">Success chance, or null for a guaranteed outcome.</param>
        /// <param name="success">Outcome on success (or always, without a chance).</param>
        /// <param name="failure">Outcome on failure; required exactly when a chance is given.</param>
        public EventChoice(string id, string textKey, ICondition condition, bool hideWhenLocked, ChanceSpec chance, EventOutcome success, EventOutcome failure)
        {
            if ((chance == null) != (failure == null))
            {
                throw new ArgumentException("A failure outcome is required exactly when a chance is given.", nameof(failure));
            }
            Id = ContentId.Require(id, nameof(id));
            TextKey = ContentId.RequireLocKey(textKey, nameof(textKey));
            Condition = condition;
            HideWhenLocked = hideWhenLocked;
            Chance = chance;
            Success = success ?? throw new ArgumentNullException(nameof(success));
            Failure = failure;
        }

        /// <summary>Choice id.</summary>
        public string Id { get; }

        /// <summary>Localization key of the button text.</summary>
        public string TextKey { get; }

        /// <summary>Requirement, or null.</summary>
        public ICondition Condition { get; }

        /// <summary>True to hide the choice when locked.</summary>
        public bool HideWhenLocked { get; }

        /// <summary>Success chance, or null.</summary>
        public ChanceSpec Chance { get; }

        /// <summary>Outcome on success.</summary>
        public EventOutcome Success { get; }

        /// <summary>Outcome on failure, or null.</summary>
        public EventOutcome Failure { get; }

        /// <summary>
        /// Returns true when the requirement holds.
        /// </summary>
        /// <param name="context">Rule context.</param>
        /// <returns>True when available.</returns>
        public bool IsAvailable(RuleContext context) => Condition == null || Condition.Evaluate(context);
    }
}
