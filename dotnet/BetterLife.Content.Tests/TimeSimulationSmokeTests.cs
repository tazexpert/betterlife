using System;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Runs ten in-game years with the real content and the default budget to catch runaway values early.
    /// Real balancing happens in the Balancing Simulator task.
    /// </summary>
    [TestFixture]
    public class TimeSimulationSmokeTests
    {
        private const int Years = 10;

        [Test]
        public void TenYears_WithDefaultBudget_KeepStatsSane()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var catalogs = content.Get<CharacterCatalogs>();
            var activities = content.Get<DefinitionCatalog<ActivityDefinition>>();
            var settings = content.Get<TimeSettings>();
            var random = new RandomRegistry(2026UL);
            var character = new Character(
                CharacterId.Create(random.GetStream("ids")),
                new PersonName("Somsri", "Rakdee"),
                GenderProfile.DefaultFor(BiologicalSex.Female),
                new BirthInfo(new CalendarDate(2000, 3, 1), 8, "thailand"),
                catalogs);
            character.Wallet.Deposit(1_000_000m);
            var bus = new EventBus();
            var state = new TimeState(new GameClock(GameDate.FromYearWeek(2026, 0)), TimeBudget.CreateDefault(activities, settings));
            var system = TimeSystemFactory.Create(content, state, random, bus, new InterruptPolicy());

            for (int year = 0; year < Years; year++)
            {
                system.Advance(character, TimeSkip.Year);
            }

            Assert.That(state.Clock.Now, Is.EqualTo(GameDate.FromYearWeek(2026 + Years, 0)));
            foreach (StatId id in catalogs.Stats.CoreStatIds)
            {
                Assert.That(character.Stats.GetValue(id), Is.InRange(StatBounds.Min, StatBounds.Max), id.Value);
            }
            float fatigue = character.Stats.GetValue(StatIds.Fatigue);
            TestContext.WriteLine($"After {Years} years: fatigue {fatigue:0.0}, health {character.Stats.GetValue(StatIds.Health):0.0}, mental {character.Stats.GetValue(StatIds.Mental):0.0}, cash {character.Wallet.Cash:0}");
            Assert.That(fatigue, Is.InRange(1f, 60f), "8 hours of sleep should keep fatigue moderate");
            Assert.That(character.History.GetSamples(StatIds.Health), Has.Count.EqualTo(StatHistory.DefaultCapacity));
        }
    }
}
