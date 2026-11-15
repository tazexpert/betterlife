using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Randomness;

namespace BetterLife.Core.Creation
{
    /// <summary>
    /// A newly created life: the character and the random registry that continues the life.
    /// </summary>
    public sealed class CreationResult
    {
        /// <summary>
        /// Creates a result.
        /// </summary>
        /// <param name="character">The new character.</param>
        /// <param name="random">Random registry of the life (same seed).</param>
        /// <param name="mode">Mode used.</param>
        public CreationResult(Character character, RandomRegistry random, CreationMode mode)
        {
            Character = character;
            Random = random;
            Mode = mode;
        }

        /// <summary>The new character.</summary>
        public Character Character { get; }

        /// <summary>Random registry of the life.</summary>
        public RandomRegistry Random { get; }

        /// <summary>Mode used.</summary>
        public CreationMode Mode { get; }

        /// <summary>Life seed.</summary>
        public ulong Seed => Random.LifeSeed;

        /// <summary>Shareable code of the seed.</summary>
        public string ChallengeCode => Creation.ChallengeCode.Encode(Random.LifeSeed);
    }
}
