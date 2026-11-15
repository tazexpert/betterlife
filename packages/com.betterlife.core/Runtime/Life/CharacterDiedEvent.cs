using BetterLife.Core.Events;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Life;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Published when a character dies.
    /// </summary>
    public sealed class CharacterDiedEvent : IGameEvent
    {
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="death">How and when.</param>
        public CharacterDiedEvent(Character character, DeathRecord death)
        {
            Character = character;
            Death = death;
        }

        /// <summary>The character.</summary>
        public Character Character { get; }

        /// <summary>How and when.</summary>
        public DeathRecord Death { get; }
    }
}
