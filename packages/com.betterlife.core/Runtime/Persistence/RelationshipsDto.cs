using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of the people in a character's life.</summary>
    [Serializable]
    public sealed class RelationshipsDto
    {
        /// <summary>NPCs.</summary>
        public List<NpcDto> Npcs = new List<NpcDto>();

        /// <summary>Relationships.</summary>
        public List<RelationshipDto> Relationships = new List<RelationshipDto>();

        /// <summary>Pregnancies.</summary>
        public List<PregnancyDto> Pregnancies = new List<PregnancyDto>();

        /// <summary>Next NPC id.</summary>
        public int NextNpcId = 1;
    }
}
