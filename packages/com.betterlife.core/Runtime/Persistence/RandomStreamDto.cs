using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Save representation of one named random stream. 64-bit values are stored as 16-digit hex strings
    /// because JSON numbers lose precision above 2^53.
    /// </summary>
    [Serializable]
    public sealed class RandomStreamDto
    {
        /// <summary>Stream name.</summary>
        public string Name;

        /// <summary>Stream seed as hex.</summary>
        public string Seed;

        /// <summary>Internal state as hex.</summary>
        public string State;

        /// <summary>Stream increment as hex.</summary>
        public string Increment;
    }
}
