using BetterLife.Core.Time;

namespace BetterLife.Core.Domain.Flags
{
    /// <summary>
    /// The current state of one flag on a character.
    /// </summary>
    public sealed class FlagEntry
    {
        /// <summary>
        /// Creates an entry.
        /// </summary>
        /// <param name="flagId">Flag id.</param>
        /// <param name="value">1 for set bool flags, the count for counters.</param>
        /// <param name="setOn">Week the flag was last set or incremented.</param>
        /// <param name="expiresOn">Week the flag clears itself, or null.</param>
        public FlagEntry(string flagId, int value, GameDate setOn, GameDate? expiresOn)
        {
            FlagId = flagId;
            Value = value;
            SetOn = setOn;
            ExpiresOn = expiresOn;
        }

        /// <summary>Flag id.</summary>
        public string FlagId { get; }

        /// <summary>1 for set bool flags, the count for counters.</summary>
        public int Value { get; }

        /// <summary>Week the flag was last set or incremented.</summary>
        public GameDate SetOn { get; }

        /// <summary>Week the flag clears itself, or null.</summary>
        public GameDate? ExpiresOn { get; }
    }
}
