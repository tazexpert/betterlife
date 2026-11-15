using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of one stored stat value.</summary>
    [Serializable]
    public sealed class StatValueDto
    {
        /// <summary>Stat id.</summary>
        public string Id;

        /// <summary>Value 0-100.</summary>
        public float Value;
    }
}
