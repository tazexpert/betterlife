using System;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a reputation entry.</summary>
    [Serializable]
    public sealed class ReputationDto
    {
        /// <summary>Domain id.</summary>
        public string DomainId;

        /// <summary>Value 0-100.</summary>
        public float Value;
    }
}
