using System.Linq;
using BetterLife.Core.Astrology;
using BetterLife.Core.Content;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Checks that the real astrology content gives fair fortunes: centred near the base and never stuck at an extreme.
    /// </summary>
    [TestFixture]
    public class FortuneDistributionTests
    {
        private const int People = 400;
        private const int Years = 24;

        [Test]
        public void Fortunes_AreCentred_AndSpread()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var calculator = content.Get<FortuneCalculator>();
            var catalogs = content.Get<CharacterCatalogs>();
            var random = new RandomRegistry(7UL).GetStream("people");

            foreach (FortuneArea area in FortuneAreas.All)
            {
                float[] values = Enumerable.Range(0, People).SelectMany(_ =>
                {
                    var birth = new CalendarDate(random.NextInt(1950, 2020), random.NextInt(1, 13), random.NextInt(1, 29));
                    var character = new Character(CharacterId.Create(random), new PersonName("A", "B"), GenderProfile.DefaultFor(BiologicalSex.Male),
                        new BirthInfo(birth, random.NextInt(0, 24), "thailand"), catalogs);
                    return Enumerable.Range(2026, Years).Select(y => calculator.Fortune(character, area, GameDate.FromYearWeek(y, 0)));
                }).ToArray();

                float mean = values.Average();
                TestContext.WriteLine($"{area}: mean {mean:0.0}, min {values.Min():0.0}, max {values.Max():0.0}");
                Assert.That(mean, Is.InRange(45f, 56f), area.ToString());
                Assert.That(values.Max() - values.Min(), Is.GreaterThan(20f), area.ToString());
                Assert.That(values, Is.All.InRange(15f, 85f), "no permanent extremes");
            }
        }
    }
}
