using System;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Persistence;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Persistence
{
    /// <summary>Tests for <see cref="CharacterDtoMapper"/>.</summary>
    [TestFixture]
    public class CharacterDtoMapperTests
    {
        private readonly GameDate _now = GameDate.FromYearWeek(2026, 10);

        [Test]
        public void RoundTrip_PreservesEverything()
        {
            CharacterCatalogs catalogs = TestCatalogs.All();
            Character original = BuildRichCharacter(catalogs);

            CharacterLoadResult loaded = CharacterDtoMapper.FromDto(CharacterDtoMapper.ToDto(original), catalogs);
            Character copy = loaded.Character;

            Assert.That(loaded.Warnings, Is.Empty);
            Assert.That(copy.Id, Is.EqualTo(original.Id));
            Assert.That(copy.Name.FamilyName, Is.EqualTo("Jaidee"));
            Assert.That(copy.Gender.Identity, Is.EqualTo(GenderIdentity.NonBinary));
            Assert.That(copy.Gender.SpeechStyle, Is.EqualTo(SpeechStyle.Neutral));
            Assert.That(copy.Birth.Date, Is.EqualTo(original.Birth.Date));
            Assert.That(copy.Birth.Hour, Is.EqualTo(9));
            foreach (StatDefinition definition in catalogs.Stats.Definitions)
            {
                Assert.That(copy.Stats.GetValue(definition.Id), Is.EqualTo(original.Stats.GetValue(definition.Id)), definition.Id.Value);
            }
            Assert.That(copy.Reputation.Get("business"), Is.EqualTo(40f));
            Assert.That(copy.Conditions.Single().ConditionId, Is.EqualTo("flu"));
            Assert.That(copy.Wallet.Cash, Is.EqualTo(12345.67m));
            Assert.That(copy.Traits.Instances.Select(t => (t.TraitId, t.IsRevealed)), Is.EqualTo(new[] { ("anxious", true), ("athletic", true) }));
            Assert.That(copy.Skills.GetLevel("programming"), Is.EqualTo(original.Skills.GetLevel("programming")));
            Assert.That(copy.Skills.Find("programming").LastPracticedOn, Is.EqualTo(_now));
            Assert.That(copy.Flags.GetCount("vice.gambling_wins"), Is.EqualTo(3));
            Assert.That(copy.Flags.Find("relationship.recent_breakup").ExpiresOn, Is.EqualTo(_now.AddWeeks(26)));
            Assert.That(copy.Flags.Find("career.fired_once").ExpiresOn, Is.Null);
            Assert.That(copy.ChangeLog.Entries, Has.Count.EqualTo(original.ChangeLog.Entries.Count));
            Assert.That(copy.History.GetSamples(StatIds.Wealth), Is.EqualTo(original.History.GetSamples(StatIds.Wealth)));
            Assert.That(copy.History.LastRecordedWeek, Is.EqualTo(_now));
        }

        [Test]
        public void Save_DoesNotContainDerivedStats()
        {
            CharacterDto dto = CharacterDtoMapper.ToDto(TestCatalogs.NewCharacter());

            Assert.That(dto.Stats.Select(s => s.Id), Has.None.EqualTo("health"));
            Assert.That(dto.Stats.Select(s => s.Id), Has.Some.EqualTo("wealth"));
        }

        [Test]
        public void Load_SkipsRemovedContent_WithWarnings()
        {
            CharacterDto dto = CharacterDtoMapper.ToDto(BuildRichCharacter(TestCatalogs.All()));
            dto.Traits.Add(new TraitDto { Id = "removed_trait" });
            dto.Skills.Add(new SkillDto { Id = "removed_skill" });
            dto.Flags.Add(new FlagDto { Id = "removed.flag", Value = 1 });
            dto.Stats.Add(new StatValueDto { Id = "removed.stat", Value = 10f });
            dto.Stats.Add(new StatValueDto { Id = "health", Value = 10f });
            dto.Reputation.Add(new ReputationDto { DomainId = "politics", Value = 5f });
            dto.Conditions.Add(new HealthConditionDto { Id = "plague", Severity = 5f });

            CharacterLoadResult loaded = CharacterDtoMapper.FromDto(dto, TestCatalogs.All());

            Assert.That(loaded.Warnings, Has.Count.EqualTo(7));
            Assert.That(loaded.Character.Traits.Instances, Has.Count.EqualTo(2));
        }

        [Test]
        public void Load_RejectsStructurallyInvalidData()
        {
            CharacterCatalogs catalogs = TestCatalogs.All();

            CharacterDto badCash = CharacterDtoMapper.ToDto(TestCatalogs.NewCharacter());
            badCash.Cash = "lots";
            CharacterDto badEnum = CharacterDtoMapper.ToDto(TestCatalogs.NewCharacter());
            badEnum.GenderIdentity = "7";
            CharacterDto badId = CharacterDtoMapper.ToDto(TestCatalogs.NewCharacter());
            badId.Id = "player";
            CharacterDto badSkill = CharacterDtoMapper.ToDto(TestCatalogs.NewCharacter());
            badSkill.Skills.Add(new SkillDto { Id = "cooking", Level = 50f, Peak = 10f });

            Assert.Throws<FormatException>(() => CharacterDtoMapper.FromDto(badCash, catalogs));
            Assert.Throws<FormatException>(() => CharacterDtoMapper.FromDto(badEnum, catalogs));
            Assert.Throws<FormatException>(() => CharacterDtoMapper.FromDto(badId, catalogs));
            Assert.Throws<FormatException>(() => CharacterDtoMapper.FromDto(badSkill, catalogs));
            Assert.Throws<ArgumentNullException>(() => CharacterDtoMapper.FromDto(null, catalogs));
        }

        private Character BuildRichCharacter(CharacterCatalogs catalogs)
        {
            Character character = TestCatalogs.NewCharacter(catalogs);
            character.UpdateGender(character.Gender.WithIdentity(GenderIdentity.NonBinary));
            character.Traits.Add("anxious", _now);
            character.Traits.Reveal("anxious");
            character.Traits.Add("athletic", _now);
            character.ApplyStatChange(new StatChange(StatIds.Stress, 10f, "source.work"), _now);
            character.ApplyStatChange(new StatChange(StatIds.Wealth, 15f, "source.salary"), _now);
            character.Reputation.Change("business", 40f);
            character.SetCondition(new HealthCondition("flu", 30f, _now));
            character.Wallet.Deposit(12345.67m);
            character.Skills.Practice("programming", 33f, _now);
            character.Flags.Set("career.fired_once", _now);
            character.Flags.Set("relationship.recent_breakup", _now);
            character.Flags.Increment("vice.gambling_wins", _now, 3);
            character.RecordWeeklyHistory(_now.AddWeeks(-1));
            character.RecordWeeklyHistory(_now);
            return character;
        }
    }
}
