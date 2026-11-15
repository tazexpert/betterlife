using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;

namespace BetterLife.Core.Relationships
{
    /// <summary>
    /// Creates NPCs with names from the country's name list and genes from the genetics model.
    /// </summary>
    public sealed class NpcFactory
    {
        private const float HalfChance = 0.5f;

        private readonly GeneticsService _genetics;
        private readonly DefinitionCatalog<NameList> _names;
        private readonly IRandomProvider _random;

        /// <summary>
        /// Creates the factory.
        /// </summary>
        /// <param name="genetics">Genetics model.</param>
        /// <param name="names">Name lists per country.</param>
        /// <param name="random">Random stream for NPCs.</param>
        public NpcFactory(GeneticsService genetics, DefinitionCatalog<NameList> names, IRandomProvider random)
        {
            _genetics = genetics ?? throw new ArgumentNullException(nameof(genetics));
            _names = names ?? throw new ArgumentNullException(nameof(names));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <summary>
        /// Turns a parent record from character creation into an NPC (born at a random week of their birth year).
        /// </summary>
        /// <param name="id">NPC id.</param>
        /// <param name="parent">Parent record.</param>
        /// <returns>The NPC.</returns>
        public Npc FromParent(int id, ParentRecord parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }
            GameDate born = GameDate.FromYearWeek(parent.BirthYear, _random.NextInt(0, GameCalendar.WeeksPerYear));
            return new Npc(id, parent.Name, parent.Sex, born, parent.Genome, parent.TraitIds);
        }

        /// <summary>
        /// Creates an unrelated person with founder genes.
        /// </summary>
        /// <param name="id">NPC id.</param>
        /// <param name="countryId">Country for the name.</param>
        /// <param name="bornOn">Birth week.</param>
        /// <returns>The NPC.</returns>
        public Npc Stranger(int id, string countryId, GameDate bornOn)
        {
            BiologicalSex sex = RollSex();
            NameList names = Names(countryId);
            return new Npc(id, new PersonName(Pick(names.GivenNames(sex)), Pick(names.Family)), sex, bornOn,
                _genetics.RollFounder(_random), _genetics.RollFounderTraits(_random));
        }

        /// <summary>
        /// Creates a child who inherits from two parents.
        /// </summary>
        /// <param name="id">NPC id.</param>
        /// <param name="countryId">Country for the given name.</param>
        /// <param name="familyName">Family name.</param>
        /// <param name="bornOn">Birth week.</param>
        /// <param name="parentGenomes">Both parents' genes.</param>
        /// <param name="parentTraits">Both parents' traits.</param>
        /// <returns>The NPC.</returns>
        public Npc Child(int id, string countryId, string familyName, GameDate bornOn, IReadOnlyList<Genome> parentGenomes,
            IReadOnlyList<IReadOnlyList<string>> parentTraits)
        {
            BiologicalSex sex = RollSex();
            return new Npc(id, new PersonName(Pick(Names(countryId).GivenNames(sex)), familyName), sex, bornOn,
                _genetics.Inherit(parentGenomes, _random), _genetics.InheritTraits(parentTraits, _random));
        }

        /// <summary>
        /// Creates an adopted child (founder genes) who takes the family name.
        /// </summary>
        /// <param name="id">NPC id.</param>
        /// <param name="countryId">Country for the given name.</param>
        /// <param name="familyName">Family name.</param>
        /// <param name="bornOn">Birth week.</param>
        /// <returns>The NPC.</returns>
        public Npc Adopted(int id, string countryId, string familyName, GameDate bornOn)
        {
            Npc stranger = Stranger(id, countryId, bornOn);
            return new Npc(id, new PersonName(stranger.Name.GivenName, familyName), stranger.Sex, bornOn, stranger.Genome, stranger.TraitIds);
        }

        private BiologicalSex RollSex() => _random.Chance(HalfChance) ? BiologicalSex.Female : BiologicalSex.Male;

        private NameList Names(string countryId) => countryId != null && _names.TryGet(countryId, out NameList list) ? list : _names.All.First();

        private string Pick(IReadOnlyList<NameEntry> pool) => pool[_random.NextInt(0, pool.Count)].In(NameScript.Native);
    }
}
