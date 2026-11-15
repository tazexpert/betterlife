using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of family info.</summary>
    [Serializable]
    public sealed class FamilyDto
    {
        /// <summary>Family background id.</summary>
        public string BackgroundId;

        /// <summary>Parents.</summary>
        public List<ParentDto> Parents = new List<ParentDto>();
    }
}
