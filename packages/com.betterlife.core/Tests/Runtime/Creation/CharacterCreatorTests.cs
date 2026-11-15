using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Domain.World;
using BetterLife.Core.Persistence;
using BetterLife.Core.Rules;
using BetterLife.Core.Rules.Conditions;
using BetterLife.Core.Tests.Content;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Creation
{
    /// <summary>Tests for <see cref="CharacterCreator"/>.</summary>
    [TestFixture]
    public class CharacterCreatorTests
    {
        private static readonly StatId Fitness = new StatId("health.fitness");
        private static readonly StatId Stress = new StatId("mental.stress");

        private IContentRepository _content;
        private CharacterCreator _creator;

        [OneTimeSetUp]
        public void LoadContent()
        {
            _content = TestContent.Load(TestContent.Source());
            _creator = new CharacterCreator(_content);
        }

        [Test]
        public void SameSeed_GivesTheSameLife()
        {
            Character a = _creator.Create(new CreationRequest(42UL, CreationMode.Challenge)).Character;
            Character b = _creator.Create(new CreationRequest(42UL, CreationMode.Random)).Character;

            Assert.That(Describe(a), Is.EqualTo(Describe(b)));
        }

        [Test]
        public void DifferentSeeds_GiveDifferentLives()
        {
            var lives = Enumerable.Range(0, 50).Select(i => Describe(_creator.Create(new CreationRequest((ulong)i, CreationMode.Random)).Character)).ToList();

            Assert.That(lives.Distinct().Count(), Is.EqualTo(50));
        }

        [Test]
        public void RandomLife_IsComplete_AndWithinSettings()
        {
            CreationResult result = _creator.Create(new CreationRequest(7UL, CreationMode.Random));
            Character c = result.Character;

            Assert.That(c.Birth.Date.Year, Is.InRange(1980, 2020));
            Assert.That(c.Family.Parents.Select(p => p.Sex), Is.EqualTo(new[] { BiologicalSex.Female, BiologicalSex.Male }));
            Assert.That(c.Family.Parents.Select(p => c.Birth.Date.Year - p.BirthYear), Is.All.InRange(20, 40));
            Assert.That(c.Family.Parents.Select(p => p.Name.FamilyName), Is.All.EqualTo(c.Name.FamilyName));
            Assert.That(c.Genome.Get(Fitness), Is.Not.Null);
            Assert.That(c.Residence.CityId, Is.Not.Null);
            Assert.That(result.ChallengeCode, Is.EqualTo(ChallengeCode.Encode(7UL)));
            Assert.That(result.Random.LifeSeed, Is.EqualTo(7UL));
        }

        [Test]
        public void BirthAdjustments_AreAppliedWithSources()
        {
            var request = new CreationRequest(5UL, CreationMode.Custom) { FamilyBackgroundId = "wealthy", MotherPersonalityId = "strict", FatherPersonalityId = "strict" };
            Character c = _creator.Create(request).Character;

            Assert.That(c.Stats.GetValue(Fitness), Is.EqualTo(Math.Min(100f, c.Genome.Get(Fitness).Value + 5f)).Within(0.001f), "genes, then +5 from a wealthy family");
            float multiplier = c.Traits.GetMultiplier(Stress, 5f);
            Assert.That(c.Stats.GetValue(Stress), Is.EqualTo(20f + 10f * multiplier).Within(0.001f), "20 + 5 + 5 from two strict parents, scaled by inherited traits");
            Assert.That(c.ChangeLog.Entries.Select(e => e.SourceKey), Is.SupersetOf(new[] { "family.wealthy.name", "parent.strict.name" }));
            Assert.That(c.Family.BackgroundId, Is.EqualTo("wealthy"));
        }

        [Test]
        public void CustomOverrides_AreUsed()
        {
            var request = new CreationRequest(9UL, CreationMode.Custom)
            {
                GivenName = "Ploy", FamilyName = "Test", Sex = BiologicalSex.Male, GenderIdentity = GenderIdentity.NonBinary,
                SpeechStyle = SpeechStyle.Feminine, BirthDate = new CalendarDate(2001, 2, 3), BirthHour = 20, CityId = "japan.tokyo",
            };

            Character c = _creator.Create(request).Character;

            Assert.That((c.Name.GivenName, c.Name.FamilyName), Is.EqualTo(("Ploy", "Test")));
            Assert.That((c.Gender.Sex, c.Gender.Identity, c.Gender.SpeechStyle), Is.EqualTo((BiologicalSex.Male, GenderIdentity.NonBinary, SpeechStyle.Feminine)));
            Assert.That((c.Birth.Date, c.Birth.Hour), Is.EqualTo((new CalendarDate(2001, 2, 3), 20)));
            Assert.That((c.Residence.CountryId, c.Residence.CityId), Is.EqualTo(("japan", "japan.tokyo")), "country follows the chosen city");
            Assert.That(c.Family.Parents.All(p => p.Name.FamilyName == "Test"), Is.True);
        }

        [Test]
        public void OverridingOneAspect_DoesNotChangeTheOthers()
        {
            Character plain = _creator.Create(new CreationRequest(77UL, CreationMode.Custom)).Character;
            Character moved = _creator.Create(new CreationRequest(77UL, CreationMode.Custom) { CountryId = "japan" }).Character;
            Character older = _creator.Create(new CreationRequest(77UL, CreationMode.Custom) { BirthDate = new CalendarDate(1985, 1, 1) }).Character;

            Assert.That(moved.Birth.Date, Is.EqualTo(plain.Birth.Date));
            Assert.That(moved.Genome.Get(Fitness), Is.EqualTo(plain.Genome.Get(Fitness)));
            Assert.That(moved.Family.BackgroundId, Is.EqualTo(plain.Family.BackgroundId));
            Assert.That(older.Residence.CityId, Is.EqualTo(plain.Residence.CityId));
            Assert.That(older.Genome.Get(Fitness), Is.EqualTo(plain.Genome.Get(Fitness)));
            Assert.That(older.Name.GivenName, Is.EqualTo(plain.Name.GivenName));
        }

        [Test]
        public void Validation_ReportsEveryProblem()
        {
            var request = new CreationRequest(1UL, CreationMode.Custom)
            {
                BirthDate = new CalendarDate(1950, 1, 1), BirthHour = 24, CountryId = "atlantis", CityId = "thailand.bangkok",
                FamilyBackgroundId = "royal", MotherPersonalityId = "chill", GivenName = new string('x', PersonName.MaxPartLength + 1),
            };

            var issues = _creator.Validate(request).Select(i => (i.Field, i.MessageKey)).ToList();

            Assert.That(issues, Is.EquivalentTo(new[]
            {
                ("BirthDate", CreationIssue.BirthYearOutOfRange), ("BirthHour", CreationIssue.BirthHourInvalid),
                ("CountryId", CreationIssue.UnknownId), ("CityId", CreationIssue.CityNotInCountry),
                ("FamilyBackgroundId", CreationIssue.UnknownId), ("MotherPersonalityId", CreationIssue.UnknownId),
                ("GivenName", CreationIssue.NameInvalid),
            }));
            Assert.Throws<InvalidOperationException>(() => _creator.Create(request));
        }

        [TestCase(CreationMode.Random)]
        [TestCase(CreationMode.Challenge)]
        public void OnlyCustomMode_AcceptsOverrides(CreationMode mode)
        {
            var request = new CreationRequest(1UL, mode) { CountryId = "thailand" };

            Assert.That(_creator.Validate(request).Single().MessageKey, Is.EqualTo(CreationIssue.OverridesNotAllowed));
        }

        [Test]
        public void Names_UseTheRequestedScript()
        {
            Character native = _creator.Create(new CreationRequest(3UL, CreationMode.Custom) { CountryId = "thailand" }).Character;
            var latinRequest = new CreationRequest(3UL, CreationMode.Custom) { CountryId = "thailand" };
            latinRequest.NameScript = NameScript.Latin;
            Character latin = _creator.Create(latinRequest).Character;

            Assert.That(native.Name.FamilyName, Is.AnyOf("ใจดี", "ศรีสุข"));
            Assert.That(latin.Name.FamilyName, Is.AnyOf("Jaidee", "Srisuk"));
        }

        [Test]
        public void ManySeeds_FollowContentWeights()
        {
            const int Lives = 2000;
            List<Character> lives = Enumerable.Range(0, Lives).Select(i => _creator.Create(new CreationRequest((ulong)(1000 + i), CreationMode.Random)).Character).ToList();

            Assert.That(lives.Count(c => c.Residence.CountryId == "thailand") / (double)Lives, Is.EqualTo(0.9).Within(0.03));
            Assert.That(lives.Count(c => c.Family.BackgroundId == "poor") / (double)Lives, Is.EqualTo(0.75).Within(0.04));
            Assert.That(lives.Count(c => c.Gender.Sex == BiologicalSex.Female) / (double)Lives, Is.EqualTo(0.5).Within(0.04));
            Assert.That(lives.Where(c => c.Residence.CountryId == "thailand").Count(c => c.Residence.CityId == "thailand.bangkok")
                / (double)lives.Count(c => c.Residence.CountryId == "thailand"), Is.EqualTo(0.75).Within(0.04));
        }

        [Test]
        public void LocationModifiers_CityOverridesCountry()
        {
            Character c = _creator.Create(new CreationRequest(1UL, CreationMode.Custom) { CityId = "thailand.bangkok" }).Character;

            LocationModifiers modifiers = c.LocationModifiers();

            Assert.That(modifiers.CostOfLiving, Is.EqualTo(1.35f));
            Assert.That(modifiers.Education, Is.EqualTo(60f), "inherited from the country");
            Assert.Throws<ArgumentException>(() => c.MoveTo(new Residence("japan", "thailand.bangkok")));
        }

        [Test]
        public void NewConditions_ReadResidenceAndFamily()
        {
            Character c = _creator.Create(new CreationRequest(1UL, CreationMode.Custom) { CityId = "thailand.bangkok", FamilyBackgroundId = "poor" }).Character;
            var context = new RuleContext(c, c.Birth.Week, new BetterLife.Core.Randomness.Pcg32(1UL, 1UL), "source.test");

            Assert.That(new LocationCondition("thailand", null).Evaluate(context), Is.True);
            Assert.That(new LocationCondition(null, "japan.tokyo").Evaluate(context), Is.False);
            Assert.That(new FamilyBackgroundCondition("poor").Evaluate(context), Is.True);
            Assert.That(new NotCondition(new FamilyBackgroundCondition("wealthy")).Evaluate(context), Is.True);
        }

        [Test]
        public void CreatedCharacter_RoundTripsThroughSave()
        {
            Character original = _creator.Create(new CreationRequest(31UL, CreationMode.Random)).Character;

            CharacterLoadResult loaded = CharacterDtoMapper.FromDto(CharacterDtoMapper.ToDto(original), _content.Get<CharacterCatalogs>());

            Assert.That(loaded.Warnings, Is.Empty);
            Assert.That(Describe(loaded.Character), Is.EqualTo(Describe(original)));
            ParentRecord mother = loaded.Character.Family.Parents[0];
            Assert.That(mother.Genome.Get(Fitness), Is.EqualTo(original.Family.Parents[0].Genome.Get(Fitness)));
            Assert.That(mother.TraitIds, Is.EqualTo(original.Family.Parents[0].TraitIds));
        }

        private static string Describe(Character c)
        {
            return string.Join("|", c.Id, c.Name.GivenName, c.Name.FamilyName, c.Gender.Sex, c.Birth.Date, c.Birth.Hour, c.Residence.CountryId, c.Residence.CityId,
                c.Family.BackgroundId, string.Join(",", c.Family.Parents.Select(p => p.PersonalityId + ":" + p.BirthYear)),
                string.Join(",", c.Genome.Values.Select(g => g.Value.ToString("0.000"))), string.Join(",", c.Traits.Instances.Select(t => t.TraitId)),
                c.Stats.GetValue(Fitness).ToString("0.000"), c.Stats.GetValue(Stress).ToString("0.000"));
        }
    }
}
