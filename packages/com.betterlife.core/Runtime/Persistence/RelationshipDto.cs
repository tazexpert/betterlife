using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a relationship.</summary>
    [Serializable]
    public sealed class RelationshipDto
    {
        /// <summary>NPC id.</summary>
        public int NpcId;

        /// <summary>Relationship type (enum name).</summary>
        public string Type;

        /// <summary>Value −100..100.</summary>
        public float Value;

        /// <summary>First week in the role (GameDate.TotalWeeks).</summary>
        public int SinceWeek;

        /// <summary>Last interaction (GameDate.TotalWeeks).</summary>
        public int LastContactWeek;

        /// <summary>Consecutive unhappy months.</summary>
        public int LowMonths;

        /// <summary>Memories.</summary>
        public List<MemoryDto> Memories = new List<MemoryDto>();

        /// <summary>Last use week per interaction.</summary>
        public List<AttemptDto> Cooldowns = new List<AttemptDto>();
    }
}
