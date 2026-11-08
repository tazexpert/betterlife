using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a recent tone.</summary>
    [Serializable]
    public sealed class ToneEntryDto
    {
        /// <summary>Week.</summary>
        public int Week;

        /// <summary>EventTone name.</summary>
        public string Tone;
    }
}
