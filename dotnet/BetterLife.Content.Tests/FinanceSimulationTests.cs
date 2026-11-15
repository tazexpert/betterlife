using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Events;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Plays lives with the real content up to age 30 without any player decisions and checks finance invariants.
    /// Nobody applies for university or jobs here, so living on the independence gift and then debt is the expected
    /// outcome; <see cref="CareerSimulationTests"/> plays with a player policy.
    /// </summary>
    [TestFixture]
    public class FinanceSimulationTests
    {
        private const int Lives = 30;
        private const int ChildYears = 17;
        private const int AdultYears = 13;

        [Test]
        public void Lives_StayDebtFreeAsChildren_AndKeepFinanceValuesInRange()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var creator = new CharacterCreator(content);
            decimal cap = content.Get<EconomySettings>().MaxDebtBalance;
            var netWorthAt30 = new List<decimal>();
            int sharks = 0;

            for (int i = 0; i < Lives; i++)
            {
                CreationResult creation = creator.Create(new CreationRequest((ulong)(4200 + i * 17), CreationMode.Random));
                using (LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus()))
                {
                    for (int year = 0; year < ChildYears && !life.IsOver; year++)
                    {
                        life.Advance(TimeSkip.Year);
                    }
                    if (life.IsOver)
                    {
                        continue;
                    }
                    Assert.That(life.Character.Finances.Debts.Debts, Is.Empty, "children never owe money");
                    Assert.That(life.Character.Finances.LifestyleTierId, Is.Null);

                    for (int year = 0; year < AdultYears && !life.IsOver; year++)
                    {
                        life.Advance(TimeSkip.Year);
                    }
                    if (life.IsOver)
                    {
                        continue;
                    }
                    Assert.That(life.Character.Finances.LifestyleTierId, Is.Not.Null);
                    Assert.That(life.Character.Finances.Debts.Debts.All(d => d.Balance <= cap), Is.True);
                    Assert.That(life.Character.Stats.GetValue(StatIds.Wealth), Is.InRange(0f, 100f));
                    Assert.That(life.Economy.State.PriceLevel, Is.GreaterThan(0m));
                    netWorthAt30.Add(life.Character.NetWorth);
                    sharks += life.Character.Finances.Debts.Debts.Any(d => d.Kind == DebtKind.Informal) ? 1 : 0;
                }
            }

            Assert.That(netWorthAt30, Is.Not.Empty);
            List<decimal> sorted = netWorthAt30.OrderBy(v => v).ToList();
            TestContext.WriteLine($"Net worth at 30 (real 2026 baht): min {sorted.First():N0}, median {sorted[sorted.Count / 2]:N0}, max {sorted.Last():N0}; "
                + $"owing a loan shark: {sharks}/{sorted.Count}");
        }
    }
}
