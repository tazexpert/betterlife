using System;
using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Core.Tests.Astrology
{
    /// <summary>Tests for <see cref="AstrologyTables"/> and <see cref="FortuneCalculator"/>.</summary>
    [TestFixture]
    public class AstrologyTests
    {
        private static readonly GameDate Year2026 = GameDate.FromYearWeek(2026, 10);

        [TestCase(4, 13, "aries")]
        [TestCase(4, 12, "pisces")]
        [TestCase(1, 13, "sagittarius")]
        [TestCase(1, 14, "capricorn")]
        [TestCase(12, 31, "sagittarius")]
        [TestCase(1, 1, "sagittarius")]
        [TestCase(6, 15, "gemini")]
        public void Sign_UsesThaiStartDays_AndWrapsAtYearEnd(int month, int day, string expected)
        {
            Assert.That(TestAstrology.Tables().SignFor(new CalendarDate(2001, month, day)).Id, Is.EqualTo(expected));
        }

        [TestCase(2020, "rat")]
        [TestCase(2026, "horse")]
        [TestCase(2000, "dragon")]
        [TestCase(1926, "tiger")]
        [TestCase(2031, "pig")]
        public void YearAnimal_CountsFromAnchor_InBothDirections(int year, string expected)
        {
            Assert.That(TestAstrology.Tables().AnimalFor(year).Id, Is.EqualTo(expected));
        }

        [Test]
        public void Profile_CombinesSign_ThaiDay_AnimalAndElement()
        {
            var birth = new BirthInfo(new CalendarDate(2000, 6, 14), 20, "thailand");

            AstrologyProfile profile = TestAstrology.Tables().ProfileFor(birth);

            Assert.That(profile.Zodiac.Id, Is.EqualTo("gemini"));
            Assert.That(profile.ZodiacIndex, Is.EqualTo(2));
            Assert.That(profile.ThaiDay, Is.EqualTo(ThaiDayOfWeek.WednesdayNight), "2000-06-14 20:00 was a Wednesday evening");
            Assert.That(profile.YearAnimal.Id, Is.EqualTo("dragon"));
            Assert.That(profile.Element.Id, Is.EqualTo("earth"));
        }

        [Test]
        public void Tables_RejectIncompleteContent()
        {
            AstrologyTables valid = TestAstrology.Tables();
            var signs = valid.Signs.ToList();
            var animals = valid.YearAnimals.ToList();
            var elements = new[] { new ElementDefinition("water", "element.water.name", FortuneModifiers.None) };

            Assert.Throws<ArgumentException>(() => new AstrologyTables(50f, valid.Cycle, 0f, 2020, signs.Take(11), new BirthDayDefinition[0], elements, animals), "11 signs");
            Assert.Throws<ArgumentException>(() => new AstrologyTables(50f, valid.Cycle, 0f, 2020, signs, new BirthDayDefinition[0], elements, animals), "missing birth days");
            Assert.Throws<ArgumentOutOfRangeException>(() => new ZodiacSignDefinition("x", "x.name", 2, 30, FortuneModifiers.None));
            Assert.Throws<ArgumentOutOfRangeException>(() => new FortuneCycle(0f, 1f, null));
        }

        [Test]
        public void Fortune_AddsModifiersOverTheBase()
        {
            var calculator = new FortuneCalculator(TestAstrology.Tables());
            Character aries = Born(new CalendarDate(2000, 4, 20), 12);
            Character mondayWater = Born(new CalendarDate(2008, 6, 16), 12);

            Assert.That(calculator.Fortune(aries, FortuneArea.Career, Year2026), Is.EqualTo(60f));
            Assert.That(calculator.Fortune(aries, FortuneArea.Love, Year2026), Is.EqualTo(50f));
            FortuneBreakdown love = calculator.Explain(mondayWater, FortuneArea.Love, Year2026);
            Assert.That(love.BirthDay, Is.EqualTo(5f), "2008-06-16 was a Monday");
            Assert.That(calculator.Fortune(mondayWater, FortuneArea.Study, Year2026), Is.EqualTo(53f), "2008 is a rat (water) year");
        }

        [Test]
        public void Cycle_RisesAndFalls_OverTwelveYears_AndStaysWithinAmplitude()
        {
            var calculator = new FortuneCalculator(TestAstrology.Tables(amplitude: 8f));
            Character character = Born(new CalendarDate(2001, 9, 20), 10);

            float[] values = Enumerable.Range(2026, 12).Select(y => calculator.Explain(character, FortuneArea.Health, GameDate.FromYearWeek(y, 0)).Cycle).ToArray();

            Assert.That(values, Is.All.InRange(-8f, 8f));
            Assert.That(values.Max() - values.Min(), Is.GreaterThan(12f), "a full period covers most of the swing");
            Assert.That(calculator.Explain(character, FortuneArea.Health, GameDate.FromYearWeek(2038, 0)).Cycle,
                Is.EqualTo(values[0]).Within(0.001f), "period of 12 years");
        }

        [Test]
        public void YearlyNoise_IsDeterministicPerCharacterYearAndArea_AndBounded()
        {
            var calculator = new FortuneCalculator(TestAstrology.Tables(noise: 5f));
            var again = new FortuneCalculator(TestAstrology.Tables(noise: 5f));
            Character character = Born(new CalendarDate(2001, 9, 20), 10);

            float a = calculator.Explain(character, FortuneArea.Finance, Year2026).Yearly;
            float b = again.Explain(character, FortuneArea.Finance, Year2026).Yearly;
            float[] years = Enumerable.Range(2026, 20).Select(y => calculator.Explain(character, FortuneArea.Finance, GameDate.FromYearWeek(y, 3)).Yearly).ToArray();

            Assert.That(a, Is.EqualTo(b));
            Assert.That(years, Is.All.InRange(-5f, 5f));
            Assert.That(years.Distinct().Count(), Is.GreaterThan(10));
            Assert.That(calculator.Explain(character, FortuneArea.Finance, GameDate.FromYearWeek(2026, 40)).Yearly, Is.EqualTo(a), "constant within a year");
        }

        [Test]
        public void Fortune_IsClampedToStatRange()
        {
            var calculator = new FortuneCalculator(TestAstrology.Tables(amplitude: 80f));
            Character character = Born(new CalendarDate(2001, 9, 20), 10);

            float[] values = Enumerable.Range(2026, 12).Select(y => calculator.Fortune(character, FortuneArea.Finance, GameDate.FromYearWeek(y, 0))).ToArray();

            Assert.That(values, Is.All.InRange(0f, 100f));
            Assert.That(values, Has.Some.EqualTo(100f));
            Assert.That(values, Has.Some.EqualTo(0f));
        }

        internal static Character Born(CalendarDate date, int hour)
        {
            return new Character(new CharacterId("chr_00000000000000cd"), new PersonName("A", "B"), GenderProfile.DefaultFor(BiologicalSex.Female),
                new BirthInfo(date, hour, "thailand"), TestCatalogs.All());
        }
    }
}
