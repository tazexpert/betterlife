using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a parent record.</summary>
    [Serializable]
    public sealed class ParentDto
    {
        /// <summary>Given name.</summary>
        public string GivenName;

        /// <summary>Family name.</summary>
        public string FamilyName;

        /// <summary>BiologicalSex name.</summary>
        public string Sex;

        /// <summary>Birth year.</summary>
        public int BirthYear;

        /// <summary>Personality id.</summary>
        public string PersonalityId;

        /// <summary>Genes.</summary>
        public List<StatValueDto> Genome = new List<StatValueDto>();

        /// <summary>Trait ids.</summary>
        public List<string> Traits = new List<string>();
    }
}
