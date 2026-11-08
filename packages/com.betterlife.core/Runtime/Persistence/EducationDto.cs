using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a character's schooling.</summary>
    [Serializable]
    public sealed class EducationDto
    {
        /// <summary>Current enrollment, or null.</summary>
        public EnrollmentDto Current;

        /// <summary>Completed programs.</summary>
        public List<CredentialDto> Credentials = new List<CredentialDto>();

        /// <summary>Entrance exam attempts.</summary>
        public List<AttemptDto> Attempts = new List<AttemptDto>();
    }
}
