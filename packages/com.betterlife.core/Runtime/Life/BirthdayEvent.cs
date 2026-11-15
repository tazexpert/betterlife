using BetterLife.Core.Events;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Time;

namespace BetterLife.Core.Life
{
    /// <summary>
    /// Published on every birthday.
    /// </summary>
    public sealed class BirthdayEvent : IGameEvent
    {
        /// <summary>
        /// Creates the message.
        /// </summary>
        /// <param name="character">The character.</param>
        /// <param name="age">Age reached.</param>
        /// <param name="week">Week of the birthday.</param>
        public BirthdayEvent(Character character, int age, GameDate week)
        {
            Character = character;
            Age = age;
            Week = week;
        }

        /// <summary>The character.</summary>
        public Character Character { get; }

        /// <summary>Age reached.</summary>
        public int Age { get; }

        /// <summary>Week of the birthday.</summary>
        public GameDate Week { get; }
    }
}
