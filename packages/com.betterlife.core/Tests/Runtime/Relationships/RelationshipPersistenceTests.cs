using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Genetics;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Persistence;
using BetterLife.Core.Relationships;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Relationships
{
    /// <summary>Tests for the relationships save mapper.</summary>
    [TestFixture]
    public class RelationshipPersistenceTests
    {
        private static readonly GameDate Week = GameDate.FromYearWeek(2030, 7);

        [Test]
        public void Relationships_RoundTripThroughTheCharacterSave()
        {
            Character original = TestCatalogs.NewCharacter();
            var genome = new Genome();
            genome.Set(StatIds.Looks, 71.5f);
            Relationship wife = original.Relationships.Add(id => new Npc(id, new PersonName("Suda", "Srisuk"), BiologicalSex.Female, Week.AddWeeks(-1500),
                genome, new[] { "calm" }), RelationshipType.Spouse, 66.6f, Week);
            wife.Remember(new RelationshipMemory("memory.wedding", Week, 15f));
            wife.Touch("gift", Week.AddWeeks(3));
            Relationship lost = original.Relationships.Add(id => new Npc(id, new PersonName("Lek", "Jaidee"), BiologicalSex.Male, Week.AddWeeks(-4000),
                new Genome(), null), RelationshipType.Parent, 40f, Week);
            original.Relationships.Npc(lost.NpcId).Die(Week.AddWeeks(10));
            original.Relationships.AddPregnancy(new Pregnancy(wife.NpcId, Week.AddWeeks(40)));

            Character copy = CharacterDtoMapper.FromDto(CharacterDtoMapper.ToDto(original), TestCatalogs.All()).Character;

            CharacterRelationships people = copy.Relationships;
            Npc suda = people.Npc(wife.NpcId);
            Assert.That((suda.Name.GivenName, suda.Sex, suda.BornOn, suda.Looks, suda.TraitIds.Single(), suda.IsAlive),
                Is.EqualTo(("Suda", BiologicalSex.Female, Week.AddWeeks(-1500), 71.5f, "calm", true)));
            Relationship r = people.With(wife.NpcId);
            Assert.That((r.Type, r.Value, r.Since, r.LastContact, r.Memories.Single().Key, r.Cooldowns["gift"]),
                Is.EqualTo((RelationshipType.Spouse, 66.6f, Week, Week.AddWeeks(3), "memory.wedding", Week.AddWeeks(3))));
            Assert.That(people.Npc(lost.NpcId).DiedOn, Is.EqualTo(Week.AddWeeks(10)));
            Assert.That(people.Pregnancies.Single().DueOn, Is.EqualTo(Week.AddWeeks(40)));
            Assert.That(people.NextNpcId, Is.EqualTo(3));
        }

        [Test]
        public void OldSaves_LoadEmpty_AndBrokenLinksAreFormatErrors()
        {
            CharacterDto dto = CharacterDtoMapper.ToDto(TestCatalogs.NewCharacter());
            dto.Relationships = null;
            Character empty = CharacterDtoMapper.FromDto(dto, TestCatalogs.All()).Character;

            Character original = TestCatalogs.NewCharacter();
            original.Relationships.Add(id => new Npc(id, new PersonName("A", "B"), BiologicalSex.Male, Week, new Genome(), null), RelationshipType.Friend, 1f, Week);
            CharacterDto broken = CharacterDtoMapper.ToDto(original);
            broken.Relationships.Relationships[0].NpcId = 99;

            Assert.That(empty.Relationships.Npcs, Is.Empty);
            Assert.Throws<FormatException>(() => CharacterDtoMapper.FromDto(broken, TestCatalogs.All()));
        }
    }
}
