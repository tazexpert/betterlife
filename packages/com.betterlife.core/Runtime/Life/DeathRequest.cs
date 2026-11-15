using BetterLife.Core.Events;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Time;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Message from the "character.die" effect asking the death service to end the life.
    /// </summary>
    public sealed class DeathRequest : IGameEvent
    {
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="causeId">Death cause id.</param>
        /// <param name="week">Week of death.</param>
        public DeathRequest(Character character, string causeId, GameDate week)
        {
            Character = character;
            CauseId = causeId;
            Week = week;
        }

        /// <summary>The character.</summary>
        public Character Character { get; }

        /// <summary>Death cause id.</summary>
        public string CauseId { get; }

        /// <summary>Week of death.</summary>
        public GameDate Week { get; }
    }
}
