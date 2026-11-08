using BetterLife.Core.Events;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Life;
using BetterLife.Core.Time;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Published when a character enters a new life stage.
    /// </summary>
    public sealed class LifeStageChangedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="previous">The stage left.</param>
        /// <param name="current">The stage entered.</param>
        /// <param name="week">Week of the change.</param>
        public LifeStageChangedEvent(Character character, LifeStageDefinition previous, LifeStageDefinition current, GameDate week)
        {
            Character = character;
            Previous = previous;
            Current = current;
            Week = week;
        }

        /// <summary>The character.</summary>
        public Character Character { get; }

        /// <summary>The stage left.</summary>
        public LifeStageDefinition Previous { get; }

        /// <summary>The stage entered.</summary>
        public LifeStageDefinition Current { get; }

        /// <summary>Week of the change.</summary>
        public GameDate Week { get; }
    }
}
