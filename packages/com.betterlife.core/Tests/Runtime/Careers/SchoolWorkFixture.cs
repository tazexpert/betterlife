using System.Collections.Generic;
using BetterLife.Core.Astrology;
using BetterLife.Core.Careers;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Education;
using BetterLife.Core.Events;
using BetterLife.Core.Karma;
using BetterLife.Core.Luck;
using BetterLife.Core.Randomness;
using BetterLife.Core.Tests.Content;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Budget;
using BetterLife.Core.Time.Ticking;
using BetterLife.Core.Time.Ticking.Phases;

namespace BetterLife.Core.Tests.Careers
{
    /// <summary>
    /// A character born in mid-2000 with education and career services built from the test content.
    /// </summary>
    internal sealed class SchoolWorkFixture
    {
        private static IContentRepository _content;

        public SchoolWorkFixture(int year, string family = null, ulong seed = 3UL)
        {
            IContentRepository content = Content;
            Character = TestCatalogs.NewCharacter();
            if (family != null)
            {
                Character.SetFamily(new FamilyInfo(family, new List<ParentRecord>()));
            }
            Clock = new GameClock(GameDate.FromYearWeek(year, 0));
            Bus = new EventBus();
            TimeSettings = content.Get<TimeSettings>();
            Budget = new TimeBudget(content.Get<DefinitionCatalog<ActivityDefinition>>(), TimeSettings);
            var luck = new LuckCalculator(content.Get<LuckSettings>(), content.Get<FortuneCalculator>(), content.Get<KarmaService>());
            var registry = new RandomRegistry(seed);
            EconomySettings economySettings = content.Get<EconomySettings>();
            IRandomProvider economyRandom = registry.GetStream("economy");
            Economy = new EconomyService(economySettings, EconomyService.CreateInitial(economySettings, year, economyRandom), economyRandom);
            Programs = content.Get<DefinitionCatalog<EducationProgramDefinition>>();
            Tracks = content.Get<DefinitionCatalog<CareerTrackDefinition>>();
            EducationSettings = content.Get<EducationSettings>();
            CareerSettings = content.Get<CareerSettings>();
            Education = new EducationService(Character, Programs, EducationSettings, TimeSettings, Budget, luck, registry.GetStream("education"),
                Dependency, Loans, Bus, Clock);
            Career = new CareerService(Character, Tracks, CareerSettings, Programs, Budget, TimeSettings, luck, registry.GetStream("career"),
                Economy, Bus, Clock);
            Loans.Character = Character;
        }

        public static IContentRepository Content => _content ?? (_content = TestContent.Load(TestContent.Source()));

        public Character Character { get; }

        public GameClock Clock { get; }

        public EventBus Bus { get; }

        public TimeSettings TimeSettings { get; }

        public TimeBudget Budget { get; }

        public EconomyService Economy { get; }

        public DefinitionCatalog<EducationProgramDefinition> Programs { get; }

        public DefinitionCatalog<CareerTrackDefinition> Tracks { get; }

        public EducationSettings EducationSettings { get; }

        public CareerSettings CareerSettings { get; }

        public FakeDependency Dependency { get; } = new FakeDependency();

        public FakeLoans Loans { get; } = new FakeLoans();

        public EducationService Education { get; }

        public CareerService Career { get; }

        public TickContext LastTick { get; private set; }

        /// <summary>Publishes this week's hours, runs the weekly education and career steps, and moves one week on.</summary>
        public void Week(int school = 0, int study = 0, int work = 0)
        {
            var hours = new Dictionary<string, int> { ["school"] = school, ["study"] = study, ["work"] = work };
            Bus.Publish(new TimeAllocationAppliedEvent(Clock.Now, hours, new StatChangeResult[0], 0m));
            LastTick = new TickContext(Clock.Now, Character);
            Education.ProcessWeek(LastTick);
            Career.ProcessWeek(LastTick);
            Clock.MoveTo(Clock.Now.AddWeeks(1));
        }

        /// <summary>Runs the monthly career step at the current week.</summary>
        public void Month()
        {
            LastTick = new TickContext(Clock.Now, Character);
            Career.ProcessMonth(LastTick);
        }

        /// <summary>Gives the character a job directly (no hiring roll).</summary>
        public Job GiveJob(string trackId, int level = 0, float performance = 55f, float boss = 55f, GameDate? review = null)
        {
            var job = new Job(trackId, level, Clock.Now, Clock.Now, performance, boss, 0, review);
            Character.Career.Start(job);
            Career.ReserveHours();
            return job;
        }

        /// <summary>Gives the character a completed credential.</summary>
        public void Graduate(string programId, string institutionId, float performance)
        {
            Character.Education.Restore(Character.Education.Current,
                new List<Credential>(Character.Education.Credentials) { new Credential(programId, institutionId, Clock.Now, performance) },
                Character.Education.Attempts);
        }
    }

    internal sealed class FakeDependency : IDependencyStatus
    {
        public bool Independent { get; set; }

        public bool IsIndependent(GameDate now) => Independent;
    }

    internal sealed class FakeLoans : ILoanProvider
    {
        public Character Character { get; set; }

        public bool Approve { get; set; } = true;

        public List<decimal> Taken { get; } = new List<decimal>();

        public LoanQuote Quote(string productId, decimal amount)
        {
            return new LoanQuote(Approve ? new string[0] : new[] { LoanQuote.ReasonCreditScore }, 0.01f, 1m, 0m);
        }

        public Debt TakeLoan(string productId, decimal amount)
        {
            Taken.Add(amount);
            Character.Wallet.Deposit(amount);
            return Character.Finances.Debts.Add(id => new Debt(id, productId, DebtKind.Student, amount, 0.01f, 1m, 180, 0m, null, GameDate.FromYearWeek(2030, 0)));
        }
    }
}
