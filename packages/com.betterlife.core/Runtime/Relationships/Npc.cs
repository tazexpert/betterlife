using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Time;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// A person in the character's life, simulated lightly: they age, carry genes and traits, and can die.
    /// </summary>
    public sealed class Npc
    {
        private const float Neutral = 50f;

        /// <summary>
        /// Creates or restores an NPC.
        /// </summary>
        /// <param name="id">Unique id within the life; positive.</param>
        /// <param name="name">Name.</param>
        /// <param name="sex">Biological sex.</param>
        /// <param name="bornOn">Birth week.</param>
        /// <param name="genome">Inherited potential.</param>
        /// <param name="traitIds">Traits.</param>
        /// <param name="diedOn">Death week, or null while alive.</param>
        public Npc(int id, PersonName name, BiologicalSex sex, GameDate bornOn, Genome genome, IEnumerable<string> traitIds, GameDate? diedOn = null)
        {
            if (id <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id), id, "Id must be positive.");
            }
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Sex = sex;
            BornOn = bornOn;
            Genome = genome ?? new Genome();
            TraitIds = (traitIds ?? Enumerable.Empty<string>()).Distinct(StringComparer.Ordinal).ToList();
            DiedOn = diedOn;
        }

        /// <summary>Unique id within the life.</summary>
        public int Id { get; }

        /// <summary>Name.</summary>
        public PersonName Name { get; }

        /// <summary>Biological sex.</summary>
        public BiologicalSex Sex { get; }

        /// <summary>Birth week.</summary>
        public GameDate BornOn { get; }

        /// <summary>Inherited potential.</summary>
        public Genome Genome { get; }

        /// <summary>Traits.</summary>
        public IReadOnlyList<string> TraitIds { get; }

        /// <summary>Death week, or null.</summary>
        public GameDate? DiedOn { get; private set; }

        /// <summary>True while alive.</summary>
        public bool IsAlive => !DiedOn.HasValue;

        /// <summary>Looks potential 0-100 (neutral when unknown).</summary>
        public float Looks => Genome.Get(StatIds.Looks) ?? Neutral;

        /// <summary>Charisma potential 0-100 (neutral when unknown).</summary>
        public float Charisma => Genome.Get(StatIds.Charisma) ?? Neutral;

        /// <summary>
        /// Returns the age in whole years.
        /// </summary>
        /// <param name="now">Week.</param>
        /// <returns>Age.</returns>
        public int AgeAt(GameDate now) => Math.Max(0, (now.TotalWeeks - BornOn.TotalWeeks) / GameCalendar.WeeksPerYear);

        /// <summary>
        /// Marks the NPC as dead.
        /// </summary>
        /// <param name="week">Week of death.</param>
        public void Die(GameDate week)
        {
            if (IsAlive)
            {
                DiedOn = week;
            }
        }
    }
}
