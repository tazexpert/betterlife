using System;
using System.Linq;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Domain
{
    /// <summary>Tests for <see cref="Character"/> and its value types.</summary>
    [TestFixture]
    public class CharacterTests
    {
        private readonly GameDate _now = GameDate.FromYearWeek(2026, 10);
        private Character _character;

        [SetUp]
        public void SetUp()
        {
            _character = TestCatalogs.NewCharacter();
        }

        [Test]
        public void ApplyStatChange_UsesTraitMultiplier_ClampsAndLogs()
        {
            _character.Traits.Add("anxious", _now);

            StatChangeResult result = _character.ApplyStatChange(new StatChange(StatIds.Stress, 8f, "source.work.overtime"), _now);

            Assert.That(result.RequestedDelta, Is.EqualTo(8f));
            Assert.That(result.ModifiedDelta, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(result.AppliedDelta, Is.EqualTo(10f).Within(0.0001f));
            Assert.That(result.NewValue, Is.EqualTo(30f).Within(0.0001f));
            Assert.That(_character.ChangeLog.Entries.Single().SourceKey, Is.EqualTo("source.work.overtime"));
        }

        [Test]
        public void ApplyStatChange_ReportsClampedAmount_AndSkipsLogWhenNothingChanged()
        {
            StatChangeResult first = _character.ApplyStatChange(new StatChange(StatIds.Stress, -50f, "source.vacation"), _now);
            StatChangeResult second = _character.ApplyStatChange(new StatChange(StatIds.Stress, -5f, "source.vacation"), _now);

            Assert.That(first.AppliedDelta, Is.EqualTo(-20f).Within(0.0001f));
            Assert.That(second.AppliedDelta, Is.EqualTo(0f));
            Assert.That(_character.ChangeLog.Entries, Has.Count.EqualTo(1));
        }

        [Test]
        public void ApplyStatChange_RejectsDerivedStats_AndBadSourceKeys()
        {
            Assert.Throws<InvalidOperationException>(() => _character.ApplyStatChange(new StatChange(StatIds.Health, 5f, "source.x"), _now));
            Assert.Throws<ArgumentException>(() => new StatChange(StatIds.Stress, 5f, "Not a key"));
            Assert.Throws<ArgumentException>(() => new StatChange(StatIds.Stress, float.PositiveInfinity, "source.x"));
        }

        [Test]
        public void SubStatChange_MovesDerivedCoreStat()
        {
            float before = _character.Stats.GetValue(StatIds.Health);

            _character.ApplyStatChange(new StatChange(StatIds.Fitness, 20f, "source.gym"), _now);

            Assert.That(_character.Stats.GetValue(StatIds.Health) - before, Is.EqualTo(7f).Within(0.001f), "20 × 0.35");
        }

        [Test]
        public void Explain_DerivedShowsFormula_StoredShowsRecentSources()
        {
            _character.ApplyStatChange(new StatChange(StatIds.Wealth, 5f, "source.salary"), _now);
            _character.ApplyStatChange(new StatChange(StatIds.Wealth, -2f, "source.rent"), _now);

            StatBreakdown health = _character.Explain(StatIds.Health, _now);
            StatBreakdown wealth = _character.Explain(StatIds.Wealth, _now);

            Assert.That(health.Kind, Is.EqualTo(StatBreakdownKind.Formula));
            Assert.That(wealth.Kind, Is.EqualTo(StatBreakdownKind.RecentChanges));
            Assert.That(wealth.Lines.Select(l => l.LabelKey), Is.EqualTo(new[] { "source.salary", "source.rent" }));
            Assert.That(wealth.Value, Is.EqualTo(33f).Within(0.0001f));
        }

        [Test]
        public void RecordWeeklyHistory_TracksAllFiveCoreStats()
        {
            _character.RecordWeeklyHistory(_now);

            Assert.That(_character.History.TrackedStats, Is.EquivalentTo(new[]
            {
                StatIds.Health, StatIds.Mental, StatIds.Intelligence, StatIds.Wealth, StatIds.Relationships,
            }));
            Assert.That(_character.History.GetSamples(StatIds.Wealth), Is.EqualTo(new[] { 30f }));
        }

        [Test]
        public void Conditions_AreReplacedById_AndRemovable()
        {
            _character.SetCondition(new HealthCondition("flu", 30f, _now));
            _character.SetCondition(new HealthCondition("flu", 60f, _now));

            Assert.That(_character.Conditions.Single().Severity, Is.EqualTo(60f));
            Assert.That(_character.RemoveCondition("flu"), Is.True);
            Assert.That(_character.HasCondition("flu"), Is.False);
            Assert.Throws<ArgumentOutOfRangeException>(() => new HealthCondition("flu", 101f, _now));
            Assert.Throws<System.Collections.Generic.KeyNotFoundException>(() => _character.SetCondition(new HealthCondition("plague", 10f, _now)));
        }

        [Test]
        public void Age_AndThaiBirthDay_ComeFromBirthInfo()
        {
            Assert.That(_character.AgeAt(_now), Is.EqualTo(25));
            Assert.That(_character.Birth.ThaiDay, Is.EqualTo(ThaiDayOfWeek.Thursday), "2000-06-15 09:00 was a Thursday");
            Assert.Throws<ArgumentOutOfRangeException>(() => new BirthInfo(new CalendarDate(2000, 1, 1), 24, "thailand"));
        }

        [Test]
        public void Gender_DefaultsFollowSex_AndIdentityIsAPlayerChoice()
        {
            GenderProfile male = GenderProfile.DefaultFor(BiologicalSex.Male);
            GenderProfile nonBinary = male.WithIdentity(GenderIdentity.NonBinary);
            GenderProfile custom = nonBinary.WithSpeechStyle(SpeechStyle.Feminine);

            Assert.That(male.Identity, Is.EqualTo(GenderIdentity.Man));
            Assert.That(male.SpeechStyle, Is.EqualTo(SpeechStyle.Masculine));
            Assert.That(GenderProfile.DefaultFor(BiologicalSex.Female).SpeechStyle, Is.EqualTo(SpeechStyle.Feminine));
            Assert.That(nonBinary.SpeechStyle, Is.EqualTo(SpeechStyle.Neutral));
            Assert.That(custom.Sex, Is.EqualTo(BiologicalSex.Male));
            Assert.That(custom.SpeechStyle, Is.EqualTo(SpeechStyle.Feminine));

            _character.UpdateGender(custom);
            Assert.That(_character.Gender, Is.SameAs(custom));
            Assert.Throws<InvalidOperationException>(() => _character.UpdateGender(GenderProfile.DefaultFor(BiologicalSex.Female)));
        }

        [Test]
        public void ValueTypes_ValidateInput()
        {
            Assert.Throws<ArgumentException>(() => new PersonName("  ", "Jaidee"));
            Assert.Throws<ArgumentException>(() => new PersonName(new string('a', PersonName.MaxPartLength + 1), "Jaidee"));
            Assert.That(new PersonName("  Somchai ", "Jaidee").GivenName, Is.EqualTo("Somchai"));
            Assert.Throws<ArgumentException>(() => new CharacterId("player1"));
        }

        [Test]
        public void CharacterId_IsReproducibleFromSeed()
        {
            CharacterId a = CharacterId.Create(new RandomRegistry(42UL).GetStream("ids"));
            CharacterId b = CharacterId.Create(new RandomRegistry(42UL).GetStream("ids"));

            Assert.That(a, Is.EqualTo(b));
            Assert.That(a.Value, Does.StartWith(CharacterId.Prefix));
        }

        [Test]
        public void Wallet_DepositsAndRefusesOverdraw()
        {
            _character.Wallet.Deposit(100.50m);

            Assert.That(_character.Wallet.TryWithdraw(200m), Is.False);
            Assert.That(_character.Wallet.TryWithdraw(0.50m), Is.True);
            Assert.That(_character.Wallet.Cash, Is.EqualTo(100m));
            Assert.Throws<ArgumentOutOfRangeException>(() => _character.Wallet.Deposit(-1m));
        }
    }
}
