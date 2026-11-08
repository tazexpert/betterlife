using System.Collections.Generic;
using BetterLife.Core.Content;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Economy;
using BetterLife.Core.Events;
using BetterLife.Core.Randomness;
using BetterLife.Core.Tests.Content;
using BetterLife.Core.Tests.Domain;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Tests.Economy
{
    /// <summary>
    /// A character born in 2000 with finance services built from the test content.
    /// </summary>
    internal sealed class FinanceFixture
    {
        private static IContentRepository _content;

        public FinanceFixture(int year = 2030, string family = null, ulong seed = 11UL)
        {
            Character = TestCatalogs.NewCharacter();
            if (family != null)
            {
                Character.SetFamily(new FamilyInfo(family, new List<ParentRecord>()));
            }
            Clock = new GameClock(GameDate.FromYearWeek(year, 0));
            Bus = new EventBus();
            IContentRepository content = Content;
            EconomySettings = content.Get<EconomySettings>();
            Settings = content.Get<FinanceSettings>();
            IRandomProvider random = new RandomRegistry(seed).GetStream("economy");
            Economy = new EconomyService(EconomySettings, EconomyService.CreateInitial(EconomySettings, year, random), random);
            Assets = content.Get<DefinitionCatalog<AssetDefinition>>();
            Products = content.Get<DefinitionCatalog<DebtProductDefinition>>();
            Lifestyles = content.Get<DefinitionCatalog<LifestyleTierDefinition>>();
            Finance = new FinanceService(Character, Assets, Products, Lifestyles, Settings, Economy,
                new IIncomeSource[] { new FamilyAllowanceSource(Settings) }, Bus, Clock);
        }

        public Character Character { get; }

        public GameClock Clock { get; }

        public EventBus Bus { get; }

        public EconomySettings EconomySettings { get; }

        public FinanceSettings Settings { get; }

        public EconomyService Economy { get; }

        public DefinitionCatalog<AssetDefinition> Assets { get; }

        public DefinitionCatalog<DebtProductDefinition> Products { get; }

        public DefinitionCatalog<LifestyleTierDefinition> Lifestyles { get; }

        public FinanceService Finance { get; }

        public static IContentRepository Content => _content ?? (_content = TestContent.Load(TestContent.Source()));

        /// <summary>Processes one month at the current week, then moves the clock 4 weeks on.</summary>
        public FinanceMonthReport Month()
        {
            var tick = new TickContext(Clock.Now, Character);
            FinanceMonthReport report = Finance.ProcessMonth(tick);
            LastTick = tick;
            Clock.MoveTo(Clock.Now.AddWeeks(4));
            return report;
        }

        public TickContext LastTick { get; private set; }

        /// <summary>Spends all cash.</summary>
        public void EmptyWallet()
        {
            if (Character.Wallet.Cash > 0m)
            {
                Character.Wallet.WithdrawUpTo(Character.Wallet.Cash);
            }
        }

        /// <summary>Makes the character independent (lifestyle chosen) with exactly this much cash.</summary>
        public void Adult(decimal cash)
        {
            Character.Finances.SetLifestyle(Settings.DefaultLifestyleId);
            EmptyWallet();
            if (cash > 0m)
            {
                Character.Wallet.Deposit(cash);
            }
        }
    }
}
