using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of an NPC.</summary>
    [Serializable]
    public sealed class NpcDto
    {
        /// <summary>NPC id.</summary>
        public int Id;

        /// <summary>Given name.</summary>
        public string GivenName;

        /// <summary>Family name.</summary>
        public string FamilyName;

        /// <summary>Biological sex (enum name).</summary>
        public string Sex;

        /// <summary>Birth week (GameDate.TotalWeeks).</summary>
        public int BirthWeek;

        /// <summary>Inherited potential.</summary>
        public List<StatValueDto> Genome = new List<StatValueDto>();

        /// <summary>Trait ids.</summary>
        public List<string> Traits = new List<string>();

        /// <summary>Whether alive.</summary>
        public bool Alive;

        /// <summary>Death week (GameDate.TotalWeeks) when not alive.</summary>
        public int DiedWeek;
    }
}
