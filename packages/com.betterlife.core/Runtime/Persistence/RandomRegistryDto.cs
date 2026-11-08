using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Save representation of a <see cref="BetterLife.Core.Randomness.RandomRegistry"/>.
    /// Streams are a list rather than a dictionary so Unity JsonUtility can serialize them.
    /// </summary>
    [Serializable]
    public sealed class RandomRegistryDto
    {
        /// <summary>Life seed as hex.</summary>
        public string LifeSeed;

        /// <summary>All created streams, sorted by name for stable output.</summary>
        public List<RandomStreamDto> Streams = new List<RandomStreamDto>();
    }
}
