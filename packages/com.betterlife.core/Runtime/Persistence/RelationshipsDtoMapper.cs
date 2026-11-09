using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Relationships;
using BetterLife.Core.Time;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Converts <see cref="CharacterRelationships"/> to and from <see cref="RelationshipsDto"/>.
    /// </summary>
    public static class RelationshipsDtoMapper
    {
        /// <summary>
        /// Captures the people in a character's life.
        /// </summary>
        /// <param name="relationships">Relationships.</param>
        /// <returns>The DTO.</returns>
        public static RelationshipsDto ToDto(CharacterRelationships relationships)
        {
            if (relationships == null)
            {
                throw new ArgumentNullException(nameof(relationships));
            }
            return new RelationshipsDto
            {
                Npcs = relationships.Npcs.OrderBy(n => n.Id).Select(n => new NpcDto
                {
                    Id = n.Id,
                    GivenName = n.Name.GivenName,
                    FamilyName = n.Name.FamilyName,
                    Sex = n.Sex.ToString(),
                    BirthWeek = n.BornOn.TotalWeeks,
                    Genome = CharacterDtoMapper.GenomeDto(n.Genome),
                    Traits = n.TraitIds.ToList(),
                    Alive = n.IsAlive,
                    DiedWeek = n.DiedOn?.TotalWeeks ?? 0,
                }).ToList(),
                Relationships = relationships.All.Select(r => new RelationshipDto
                {
                    NpcId = r.NpcId,
                    Type = r.Type.ToString(),
                    Value = r.Value,
                    SinceWeek = r.Since.TotalWeeks,
                    LastContactWeek = r.LastContact.TotalWeeks,
                    LowMonths = r.LowMonths,
                    Memories = r.Memories.Select(m => new MemoryDto { Key = m.Key, Week = m.Week.TotalWeeks, Delta = m.Delta }).ToList(),
                    Cooldowns = EducationDtoMapper.Attempts(r.Cooldowns),
                }).ToList(),
                Pregnancies = relationships.Pregnancies.Select(p => new PregnancyDto { PartnerNpcId = p.PartnerNpcId, DueWeek = p.DueOn.TotalWeeks }).ToList(),
                NextNpcId = relationships.NextNpcId,
            };
        }

        /// <summary>
        /// Restores saved relationships. A null DTO (an older save) leaves them empty.
        /// </summary>
        /// <param name="dto">Saved relationships, or null.</param>
        /// <param name="relationships">Target.</param>
        public static void Restore(RelationshipsDto dto, CharacterRelationships relationships)
        {
            if (relationships == null)
            {
                throw new ArgumentNullException(nameof(relationships));
            }
            if (dto == null)
            {
                return;
            }
            try
            {
                List<Npc> npcs = (dto.Npcs ?? new List<NpcDto>()).Where(n => n != null).Select(n => new Npc(n.Id, new PersonName(n.GivenName, n.FamilyName),
                    EducationDtoMapper.ParseEnum<BiologicalSex>(n.Sex, nameof(n.Sex)), new GameDate(n.BirthWeek), CharacterDtoMapper.GenomeFrom(n.Genome),
                    n.Traits, n.Alive ? (GameDate?)null : new GameDate(n.DiedWeek))).ToList();
                List<Relationship> links = (dto.Relationships ?? new List<RelationshipDto>()).Where(r => r != null).Select(r =>
                {
                    var relationship = new Relationship(r.NpcId, EducationDtoMapper.ParseEnum<RelationshipType>(r.Type, nameof(r.Type)), r.Value,
                        new GameDate(r.SinceWeek), new GameDate(r.LastContactWeek), r.LowMonths);
                    relationship.Restore((r.Memories ?? new List<MemoryDto>()).Where(m => m != null).Select(m => new RelationshipMemory(m.Key, new GameDate(m.Week), m.Delta)),
                        EducationDtoMapper.FromAttempts(r.Cooldowns));
                    return relationship;
                }).ToList();
                relationships.Restore(npcs, links,
                    (dto.Pregnancies ?? new List<PregnancyDto>()).Where(p => p != null).Select(p => new Pregnancy(p.PartnerNpcId, new GameDate(p.DueWeek))),
                    dto.NextNpcId);
            }
            catch (ArgumentException ex)
            {
                throw new FormatException("Saved relationships are invalid: " + ex.Message, ex);
            }
        }
    }
}
