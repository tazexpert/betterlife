using BetterLife.Core.Events;
using BetterLife.Core.Domain.Characters;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Published when aging gives a character a condition.
    /// </summary>
    public sealed class HealthConditionStartedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="condition">The new condition.</param>
        public HealthConditionStartedEvent(Character character, HealthCondition condition)
        {
            Character = character;
            Condition = condition;
        }

        /// <summary>The character.</summary>
        public Character Character { get; }

        /// <summary>The new condition.</summary>
        public HealthCondition Condition { get; }
    }
}
