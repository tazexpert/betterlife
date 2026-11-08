using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Traits
{
    /// <summary>
    /// A trait held by a character.
    /// </summary>
    public sealed class TraitInstance
    {
        /// <summary>
        /// Creates an instance.
        /// </summary>
        /// <param name="traitId">Trait id.</param>
        /// <param name="acquiredOn">Week the trait was gained.</param>
        /// <param name="isRevealed">Whether the player knows about it.</param>
        public TraitInstance(string traitId, GameDate acquiredOn, bool isRevealed)
        {
            TraitId = traitId;
            AcquiredOn = acquiredOn;
            IsRevealed = isRevealed;
        }

        /// <summary>Trait id.</summary>
        public string TraitId { get; }

        /// <summary>Week the trait was gained.</summary>
        public GameDate AcquiredOn { get; }

        /// <summary>Whether the player knows about it.</summary>
        public bool IsRevealed { get; private set; }

        /// <summary>
        /// Marks the trait as known to the player.
        /// </summary>
        /// <returns>True when it was hidden before.</returns>
        internal bool Reveal()
        {
            bool wasHidden = !IsRevealed;
            IsRevealed = true;
            return wasHidden;
        }
    }
}
