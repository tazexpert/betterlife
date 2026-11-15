using BetterLife.Core.Events;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Time;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Published when a condition heals by itself.
    /// </summary>
    public sealed class HealthConditionRecoveredEvent : IGameEvent
    {
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="condition">The healed condition.</param>
        /// <param name="week">Week of recovery.</param>
        public HealthConditionRecoveredEvent(Character character, HealthConditionDefinition condition, GameDate week)
        {
            Character = character;
            Condition = condition;
            Week = week;
        }

        /// <summary>The character.</summary>
        public Character Character { get; }

        /// <summary>The healed condition.</summary>
        public HealthConditionDefinition Condition { get; }

        /// <summary>Week of recovery.</summary>
        public GameDate Week { get; }
    }
}
