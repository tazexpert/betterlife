using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Careers;
using BetterLife.Core.Content;
using BetterLife.Core.Creation;
using BetterLife.Core.Domain;
using BetterLife.Core.Economy;
using BetterLife.Core.Education;
using BetterLife.Core.Events;
using BetterLife.Core.Sessions;
using BetterLife.Core.Time.Ticking;
using NUnit.Framework;

namespace BetterLife.Content.Tests
{
    /// <summary>
    /// Plays lives with the real content up to age 30 with a simple monthly player policy (apply to the best university the
    /// odds allow, then to the best-paid legal job, live within the income, repay loan sharks) and checks that schooling
    /// and work happen. Fine balancing is task 21.
    /// </summary>
    [TestFixture]
    public class CareerSimulationTests
    {
        private const int Lives = 40;
        private const int Months = 30 * 12;
        private const decimal LifestyleShareOfIncome = 0.6m;
        private const int CashBufferMonths = 2;
        private const float GoodOdds = 50f;

        [Test]
        public void Lives_GoToSchool_FindWork_AndPayTheirWay()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var creator = new CharacterCreator(content);
            var programs = content.Get<DefinitionCatalog<EducationProgramDefinition>>();
            int alive = 0, secondary = 0, degree = 0, employed = 0, sharks = 0;
            var netWorth = new List<decimal>();
            var jobs = new List<string>();

            for (int i = 0; i < Lives; i++)
            {
                CreationResult creation = creator.Create(new CreationRequest((ulong)(7100 + i * 13), CreationMode.Random));
                using (LifeSession life = LifeSessionFactory.CreateNew(content, creation, new EventBus()))
                {
                    for (int month = 0; month < Months && !life.IsOver; month++)
                    {
                        life.Advance(TimeSkip.Month);
                        if (!life.IsOver)
                        {
                            Decide(life);
                        }
                    }
                    if (life.IsOver)
                    {
                        continue;
                    }
                    alive++;
                    int rank = life.Character.Education.HighestRank(programs);
                    secondary += rank >= 4 ? 1 : 0;
                    degree += rank >= 6 ? 1 : 0;
                    employed += life.Character.Career.IsEmployed ? 1 : 0;
                    sharks += life.Character.Finances.Debts.Debts.Any(d => d.Kind == DebtKind.Informal) ? 1 : 0;
                    netWorth.Add(life.Character.NetWorth);
                    jobs.Add(life.Character.Career.Current?.TrackId ?? "none");
                }
            }

            List<decimal> sorted = netWorth.OrderBy(v => v).ToList();
            TestContext.WriteLine($"Alive at 30: {alive}; finished secondary {secondary}, degree {degree}, employed {employed}, owing a loan shark {sharks}");
            TestContext.WriteLine($"Net worth at 30: min {sorted.First():N0}, median {sorted[sorted.Count / 2]:N0}, max {sorted.Last():N0}");
            TestContext.WriteLine("Jobs: " + string.Join(", ", jobs.GroupBy(j => j).OrderByDescending(g => g.Count()).Select(g => $"{g.Key} {g.Count()}")));
            Assert.That(alive, Is.GreaterThan(Lives / 2));
            Assert.That(secondary, Is.GreaterThan(alive / 2), "most children finish secondary school");
            Assert.That(employed, Is.GreaterThan(alive / 2), "most adults find work");
            Assert.That(sorted[sorted.Count / 2], Is.GreaterThan(0m), "a working median life is not in debt");
        }

        [Test]
        public void PremiumCareers_NeverPayMoreThanNormalCareersOfTheSameCategory()
        {
            IContentRepository content = DefaultContent.CreatePipeline(new FileSystemContentSource(RepoContent.ContentDir)).Load().RequireRepository();
            var tracks = content.Get<DefinitionCatalog<CareerTrackDefinition>>().All;

            foreach (CareerTrackDefinition premium in tracks.Where(t => t.Premium))
            {
                var normal = tracks.Where(t => !t.Premium && t.Category == premium.Category).ToList();
                Assert.That(normal, Is.Not.Empty, $"{premium.Id} needs a normal career in its category to compare with");
                Assert.That(premium.Levels[0].MonthlySalary, Is.LessThanOrEqualTo(normal.Max(t => t.Levels[0].MonthlySalary)), premium.Id);
                Assert.That(premium.Levels.Max(l => l.MonthlySalary), Is.LessThanOrEqualTo(normal.Max(t => t.Levels.Max(l => l.MonthlySalary))), premium.Id);
                Assert.That(premium.IncomeVolatility, Is.LessThanOrEqualTo(normal.Max(t => t.IncomeVolatility)), premium.Id);
            }
        }

        private static void Decide(LifeSession life)
        {
            var character = life.Character;
            int age = character.AgeAt(life.TimeState.Clock.Now);
            if (age >= 17 && !character.Education.IsEnrolled && !character.Career.IsEmployed)
            {
                var option = life.Education.Options()
                    .Where(o => o.Program.Rank >= 5 && o.Quote.Eligible && (o.Quote.IsOpenAdmission || o.Quote.Chance.Final >= GoodOdds))
                    .OrderByDescending(o => o.Program.Rank).ThenByDescending(o => o.Institution.Prestige).FirstOrDefault();
                if (option.Program != null && character.Education.HighestRank(life.Education.Programs) < 6)
                {
                    TuitionFunding funding = character.Wallet.Cash >= option.Quote.TuitionPerYear * option.Program.Years
                        ? TuitionFunding.Cash : TuitionFunding.StudentLoan;
                    if (life.Education.Apply(option.Program.Id, option.Institution.Id, funding).Admitted)
                    {
                        return;
                    }
                }
            }
            ManageMoney(life);
            if (age >= 18 && !character.Education.IsEnrolled && !character.Career.IsEmployed)
            {
                foreach (CareerTrackDefinition track in life.Career.Tracks.All
                    .Where(t => t.Category != CareerCategory.Illegal && t.Category != CareerCategory.HighRisk)
                    .OrderByDescending(t => t.Levels[0].MonthlySalary))
                {
                    if (life.Career.Quote(track.Id).Eligible && life.Career.Apply(track.Id).Hired)
                    {
                        return;
                    }
                }
            }
        }

        private static void ManageMoney(LifeSession life)
        {
            var character = life.Character;
            if (character.Finances.LifestyleTierId == null)
            {
                return;
            }
            var tiers = life.Finance.LifestyleTiers.All.OrderByDescending(t => t.MonthlyCost).ToList();
            decimal budget = character.Finances.LastMonthlyIncome * LifestyleShareOfIncome;
            LifestyleTierDefinition tier = tiers.FirstOrDefault(t => life.Finance.LifestyleCost(t.Id) <= budget) ?? tiers.Last();
            life.Finance.SetLifestyle(tier.Id);
            foreach (Debt shark in character.Finances.Debts.Debts.Where(d => d.Kind == DebtKind.Informal).ToList())
            {
                decimal spare = character.Wallet.Cash - life.Finance.LifestyleCost(tier.Id) * CashBufferMonths;
                if (spare > 0m && shark.Balance > 0m)
                {
                    life.Finance.Repay(shark.Id, System.Math.Min(spare, shark.Balance));
                }
            }
        }
    }
}
