using System;
using System.Collections.Generic;
using System.Linq;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Family;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Events;
using BetterLife.Core.Time;
using BetterLife.Core.Time.Ticking;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Personal finance of the player character: monthly income, costs, investments, loans, defaults and bankruptcy,
    /// plus the actions the finance screen offers. Cash shortfalls from anywhere become family help (children)
    /// or debt (adults: card first, then the loan shark).
    /// </summary>
    public sealed class FinanceService : IDisposable, IDependencyStatus, ILoanProvider
    {
        /// <summary>Flag set while owing a loan shark.</summary>
        public const string LoanSharkFlag = "finance.loan_shark";

        /// <summary>Flag set during bankruptcy.</summary>
        public const string BankruptFlag = "finance.bankrupt";

        /// <summary>Source keys used in reports and breakdowns.</summary>
        public const string FamilyCoverKey = "finance.family_covered";
        /// <summary>Source key of the independence gift.</summary>
        public const string GiftKey = "finance.independence_gift";
        /// <summary>Source key of income tax.</summary>
        public const string TaxKey = "finance.income_tax";
        /// <summary>Source key of inflation.</summary>
        public const string InflationKey = "finance.inflation";
        /// <summary>Source key of rent income.</summary>
        public const string RentKey = "finance.rent";
        /// <summary>Source key of the Wealth update.</summary>
        public const string NetWorthKey = "finance.net_worth";
        /// <summary>Source key of debt stress.</summary>
        public const string DebtStressKey = "finance.debt_stress";
        /// <summary>Source key of on-time payments.</summary>
        public const string OnTimeKey = "finance.credit.on_time";
        /// <summary>Source key of missed payments.</summary>
        public const string MissedKey = "finance.credit.missed";
        /// <summary>Source key of defaults.</summary>
        public const string DefaultKey = "finance.credit.default";
        /// <summary>Source key of foreclosures.</summary>
        public const string ForeclosureKey = "finance.credit.foreclosure";
        /// <summary>Source key of bankruptcy.</summary>
        public const string BankruptcyKey = "finance.credit.bankruptcy";
        /// <summary>Source key of high card utilization.</summary>
        public const string UtilizationKey = "finance.credit.utilization";
        /// <summary>Interrupt reason of defaults and bankruptcy.</summary>
        public const string TroubleInterruptKey = "interrupt.finance_trouble";

        private readonly Character _character;
        private readonly DefinitionCatalog<AssetDefinition> _assets;
        private readonly DefinitionCatalog<DebtProductDefinition> _products;
        private readonly DefinitionCatalog<LifestyleTierDefinition> _lifestyles;
        private readonly FinanceSettings _settings;
        private readonly EconomyService _economy;
        private readonly IEventBus _bus;
        private readonly List<IIncomeSource> _incomeSources;
        private readonly IDisposable _subscription;
        private readonly GameClock _clock;
        private List<FinanceLine> _openReport;

        /// <summary>
        /// Creates the service for one character and starts covering its cash shortfalls.
        /// </summary>
        /// <param name="character">The player character.</param>
        /// <param name="assets">Asset catalog.</param>
        /// <param name="products">Loan products.</param>
        /// <param name="lifestyles">Lifestyle tiers.</param>
        /// <param name="settings">Finance settings.</param>
        /// <param name="economy">Economy service.</param>
        /// <param name="incomeSources">Monthly income sources.</param>
        /// <param name="bus">Event bus.</param>
        /// <param name="clock">Game clock (current week for player actions).</param>
        public FinanceService(Character character, DefinitionCatalog<AssetDefinition> assets, DefinitionCatalog<DebtProductDefinition> products,
            DefinitionCatalog<LifestyleTierDefinition> lifestyles, FinanceSettings settings, EconomyService economy,
            IEnumerable<IIncomeSource> incomeSources, IEventBus bus, GameClock clock)
        {
            _character = character ?? throw new ArgumentNullException(nameof(character));
            _assets = assets ?? throw new ArgumentNullException(nameof(assets));
            _products = products ?? throw new ArgumentNullException(nameof(products));
            _lifestyles = lifestyles ?? throw new ArgumentNullException(nameof(lifestyles));
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _clock = clock ?? throw new ArgumentNullException(nameof(clock));
            _incomeSources = (incomeSources ?? Enumerable.Empty<IIncomeSource>()).ToList();
            _products.Get(settings.FallbackProductId);
            _subscription = _bus.Subscribe<CashShortfallEvent>(e => CoverShortfall(e.Amount, e.SourceKey, e.Week));
        }

        /// <summary>The latest monthly report, or null before the first month.</summary>
        public FinanceMonthReport LastReport { get; private set; }

        /// <summary>
        /// Returns true once the character pays their own way.
        /// </summary>
        /// <param name="now">Week.</param>
        /// <returns>True when independent.</returns>
        public bool IsIndependent(GameDate now) => _character.AgeAt(now) >= _settings.IndependenceAge;

        /// <summary>
        /// Returns the monthly cost of a lifestyle where the character lives.
        /// </summary>
        /// <param name="tierId">Tier id.</param>
        /// <returns>Real monthly cost.</returns>
        public decimal LifestyleCost(string tierId) => Money.Round(_lifestyles.Get(tierId).MonthlyCost * CostOfLiving);

        /// <summary>
        /// Chooses a lifestyle (independent characters only).
        /// </summary>
        /// <param name="tierId">Tier id.</param>
        public void SetLifestyle(string tierId)
        {
            RequireIndependent();
            _lifestyles.Get(tierId);
            _character.Finances.SetLifestyle(tierId);
        }

        /// <summary>
        /// Buys an asset with cash.
        /// </summary>
        /// <param name="assetId">Asset id.</param>
        /// <param name="amount">Amount to invest; at least the asset's minimum.</param>
        /// <returns>The new holding.</returns>
        public Holding Buy(string assetId, decimal amount)
        {
            RequireIndependent();
            AssetDefinition asset = _assets.Get(assetId);
            if (amount < asset.MinPurchase)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Below the minimum purchase.");
            }
            if (!_character.Wallet.TryWithdraw(amount))
            {
                throw new InvalidOperationException("Not enough cash.");
            }
            return _character.Finances.Portfolio.Add(assetId, amount, _clock.Now);
        }

        /// <summary>
        /// Sells part or all of a holding (minus the asset's selling fee). Collateral cannot be sold while its loan is open.
        /// </summary>
        /// <param name="holdingId">Holding id.</param>
        /// <param name="share">Share to sell, (0, 1].</param>
        /// <returns>Cash received.</returns>
        public decimal Sell(int holdingId, decimal share = 1m)
        {
            if (share <= 0m || share > 1m)
            {
                throw new ArgumentOutOfRangeException(nameof(share), share, "Share must be within (0, 1].");
            }
            Holding holding = _character.Finances.Portfolio.Find(holdingId) ?? throw new ArgumentException($"No holding {holdingId}.", nameof(holdingId));
            if (_character.Finances.Debts.Debts.Any(d => d.CollateralHoldingId == holdingId))
            {
                throw new InvalidOperationException("The holding secures a loan.");
            }
            decimal gross = holding.RemoveShare(share);
            if (share == 1m || holding.Value <= 0m)
            {
                _character.Finances.Portfolio.Remove(holdingId);
            }
            decimal proceeds = Money.Round(gross * (1m - _assets.Get(holding.AssetId).SellFeeRate));
            if (proceeds > 0m)
            {
                _character.Wallet.Deposit(proceeds);
            }
            return proceeds;
        }

        /// <summary>
        /// Asks a lender for terms.
        /// </summary>
        /// <param name="productId">Loan product.</param>
        /// <param name="amount">Amount to borrow (ignored for credit cards).</param>
        /// <returns>The quote.</returns>
        public LoanQuote Quote(string productId, decimal amount)
        {
            DebtProductDefinition product = _products.Get(productId);
            GameDate now = _clock.Now;
            var reasons = new List<string>();
            if (!IsIndependent(now))
            {
                reasons.Add(LoanQuote.ReasonTooYoung);
            }
            bool formal = product.Kind != DebtKind.Informal;
            if (formal && _character.Finances.IsBankrupt(now))
            {
                reasons.Add(LoanQuote.ReasonBankrupt);
            }
            if (_character.Stats.GetValue(StatIds.CreditScore) < product.MinCreditScore)
            {
                reasons.Add(LoanQuote.ReasonCreditScore);
            }

            decimal monthlyIncome = _character.Finances.LastMonthlyIncome;
            float rate = product.RateAt(_economy.State.PolicyRate);
            if (product.IsRevolving)
            {
                decimal limit = Money.Round(monthlyIncome * (decimal)product.LimitIncomeMultiple);
                if (formal && limit <= 0m)
                {
                    reasons.Add(LoanQuote.ReasonIncome);
                }
                return new LoanQuote(reasons, rate, 0m, limit);
            }

            if (amount <= 0m)
            {
                reasons.Add(LoanQuote.ReasonAmount);
            }
            if (product.MaxIncomeMultiple.HasValue && amount > monthlyIncome * Money.MonthsPerYear * (decimal)product.MaxIncomeMultiple.Value)
            {
                reasons.Add(LoanQuote.ReasonIncome);
            }
            return new LoanQuote(reasons, rate, Installment(amount, rate, product.TermMonths), 0m);
        }

        /// <summary>
        /// Takes an unsecured installment loan and receives the cash.
        /// </summary>
        /// <param name="productId">Unsecured installment product.</param>
        /// <param name="amount">Amount.</param>
        /// <returns>The loan.</returns>
        public Debt TakeLoan(string productId, decimal amount)
        {
            DebtProductDefinition product = _products.Get(productId);
            if (product.IsRevolving || product.CollateralClass.HasValue)
            {
                throw new InvalidOperationException("Use OpenCreditLine or BuyWithLoan for this product.");
            }
            LoanQuote quote = RequireApproved(productId, amount);
            Debt debt = AddDebt(product, amount, quote.AnnualRate, quote.MonthlyPayment, 0m, null);
            _character.Wallet.Deposit(Money.Round(amount));
            return debt;
        }

        /// <summary>
        /// Opens a revolving credit line (credit card) with a limit based on income.
        /// </summary>
        /// <param name="productId">Revolving product.</param>
        /// <returns>The credit line (balance 0).</returns>
        public Debt OpenCreditLine(string productId)
        {
            DebtProductDefinition product = _products.Get(productId);
            if (!product.IsRevolving)
            {
                throw new InvalidOperationException("Not a revolving product.");
            }
            LoanQuote quote = RequireApproved(productId, 0m);
            return AddDebt(product, 0m, quote.AnnualRate, 0m, quote.CreditLimit, null);
        }

        /// <summary>
        /// Buys an asset with a secured loan: the down payment comes from cash and the asset secures the loan.
        /// </summary>
        /// <param name="assetId">Asset to buy.</param>
        /// <param name="price">Purchase price.</param>
        /// <param name="productId">Secured product whose collateral class matches the asset.</param>
        /// <returns>The loan.</returns>
        public Debt BuyWithLoan(string assetId, decimal price, string productId)
        {
            AssetDefinition asset = _assets.Get(assetId);
            DebtProductDefinition product = _products.Get(productId);
            if (product.CollateralClass != asset.Class)
            {
                throw new InvalidOperationException(LoanQuote.ReasonCollateral);
            }
            if (price < asset.MinPurchase)
            {
                throw new ArgumentOutOfRangeException(nameof(price), price, "Below the minimum purchase.");
            }
            decimal down = Money.Round(price * (decimal)product.DownPaymentRate);
            decimal principal = price - down;
            LoanQuote quote = RequireApproved(productId, principal);
            if (down > 0m && !_character.Wallet.TryWithdraw(down))
            {
                throw new InvalidOperationException(LoanQuote.ReasonCash);
            }
            Holding holding = _character.Finances.Portfolio.Add(assetId, price, _clock.Now);
            return AddDebt(product, principal, quote.AnnualRate, quote.MonthlyPayment, 0m, holding.Id);
        }

        /// <summary>
        /// Pays extra towards a loan from cash; a fully paid installment loan closes.
        /// </summary>
        /// <param name="debtId">Loan id.</param>
        /// <param name="amount">Positive amount (reduced to the balance).</param>
        /// <returns>The amount paid.</returns>
        public decimal Repay(int debtId, decimal amount)
        {
            Debt debt = _character.Finances.Debts.Find(debtId) ?? throw new ArgumentException($"No loan {debtId}.", nameof(debtId));
            decimal paid = Math.Min(amount, debt.Balance);
            if (paid <= 0m || !_character.Wallet.TryWithdraw(paid))
            {
                throw new InvalidOperationException("Nothing to pay or not enough cash.");
            }
            debt.Pay(paid);
            CloseIfPaid(debt);
            return paid;
        }

        /// <summary>
        /// Covers a payment that cash could not: the family pays for dependents; adults use card room, then the loan shark.
        /// </summary>
        /// <param name="amount">Unpaid amount; positive.</param>
        /// <param name="sourceKey">What was being paid for.</param>
        /// <param name="week">Week of the payment.</param>
        public void CoverShortfall(decimal amount, string sourceKey, GameDate week)
        {
            if (amount <= 0m || !_character.IsAlive)
            {
                return;
            }
            if (!IsIndependent(week))
            {
                Report(FinanceLineCategory.Shortfall, FamilyCoverKey, amount);
                return;
            }

            decimal remaining = amount;
            foreach (Debt card in _character.Finances.Debts.Debts.Where(d => d.CreditLimit > 0m && !d.IsDefaulted).ToList())
            {
                decimal draw = Math.Min(remaining, card.AvailableCredit);
                if (draw > 0m)
                {
                    card.Add(draw, _economy.Settings.MaxDebtBalance);
                    remaining -= draw;
                    Report(FinanceLineCategory.Shortfall, _products.Get(card.ProductId).NameKey, draw);
                }
            }
            if (remaining <= 0m)
            {
                return;
            }

            DebtProductDefinition fallback = _products.Get(_settings.FallbackProductId);
            Debt shark = _character.Finances.Debts.Debts.FirstOrDefault(d => d.ProductId == fallback.Id)
                ?? AddDebt(fallback, 0m, fallback.RateAt(_economy.State.PolicyRate), 0m, 0m, null);
            shark.Add(remaining, _economy.Settings.MaxDebtBalance);
            Report(FinanceLineCategory.Shortfall, fallback.NameKey, remaining);
            SetFlag(LoanSharkFlag, week, null);
        }

        /// <summary>
        /// Processes one month: inflation, investments, income and tax, living costs, loan payments, defaults,
        /// bankruptcy, debt stress, credit utilization and the Wealth score.
        /// </summary>
        /// <param name="tick">Tick context of the month's first week.</param>
        /// <returns>The report, or null for a dead character.</returns>
        public FinanceMonthReport ProcessMonth(TickContext tick)
        {
            if (tick == null)
            {
                throw new ArgumentNullException(nameof(tick));
            }

            MarketMonth market = _economy.RollMonth();
            if (!_character.IsAlive)
            {
                return null;
            }
            GameDate week = tick.Week;
            _openReport = new List<FinanceLine>();
            ApplyInflationToCash(market);
            ApplyInvestmentReturns(market);
            bool independent = IsIndependent(week);
            if (independent && _character.Finances.LifestyleTierId == null)
            {
                BecomeIndependent();
            }
            CollectIncome(week);
            if (independent)
            {
                PayLivingCosts(week);
            }
            ServiceDebts(market, week, tick);
            CheckBankruptcy(week, tick);
            if (independent)
            {
                ApplyDebtStress(week);
                ApplyCardUtilization(week);
                UpdateWealth(week);
            }
            if (!_character.Finances.Debts.Debts.Any(d => d.Kind == DebtKind.Informal))
            {
                ClearFlag(LoanSharkFlag);
            }

            var report = new FinanceMonthReport(week, _openReport, _character.NetWorth);
            _openReport = null;
            LastReport = report;
            _character.Finances.SetLastMonthlyIncome(report.GrossIncome);
            _bus.Publish(new FinanceMonthReportEvent(report));
            return report;
        }

        /// <summary>
        /// Stops covering shortfalls.
        /// </summary>
        public void Dispose() => _subscription.Dispose();

        private decimal CostOfLiving => (decimal)(_character.LocationModifiers().CostOfLiving ?? 1f);

        private void ApplyInflationToCash(MarketMonth market)
        {
            decimal cash = _character.Wallet.Cash;
            decimal loss = Money.Round(cash - cash / (1m + market.Inflation));
            if (loss > 0m)
            {
                _character.Wallet.WithdrawUpTo(loss);
                Report(FinanceLineCategory.Inflation, InflationKey, -loss);
            }
            foreach (Debt debt in _character.Finances.Debts.Debts)
            {
                debt.Deflate(market.Inflation);
            }
        }

        private void ApplyInvestmentReturns(MarketMonth market)
        {
            foreach (Holding holding in _character.Finances.Portfolio.Holdings.ToList())
            {
                AssetDefinition asset = _assets.Get(holding.AssetId);
                decimal before = holding.Value;
                holding.ApplyReturn(MonthlyReturn(asset, market));
                if (holding.Value != before)
                {
                    Report(FinanceLineCategory.Investment, asset.NameKey, holding.Value - before);
                }
                if (asset.RentalYield > 0f && holding.Value > 0m)
                {
                    decimal rent = Money.Round(holding.Value * (decimal)asset.RentalYield / Money.MonthsPerYear);
                    if (rent > 0m)
                    {
                        Receive(new IncomeItem(RentKey, rent, true));
                    }
                }
            }
        }

        private decimal MonthlyReturn(AssetDefinition asset, MarketMonth market)
        {
            decimal nominalToReal(float annual) => (1m + (decimal)Money.Monthly(annual)) / (1m + market.Inflation) - 1m;
            switch (asset.Class)
            {
                case AssetClass.Deposit:
                case AssetClass.Bond:
                    return nominalToReal(Math.Max(0f, market.PolicyRate + asset.RateSpread));
                case AssetClass.Fund:
                    return market.Stocks;
                case AssetClass.Stock:
                    return _economy.StockReturn(market, asset.Beta, asset.IdiosyncraticVolatility);
                case AssetClass.RealEstate:
                    return market.Property;
                case AssetClass.Vehicle:
                    return (decimal)(Math.Pow(1.0 - asset.Depreciation, 1.0 / Money.MonthsPerYear) - 1.0);
                case AssetClass.Crypto:
                    return market.Crypto;
                default:
                    throw new ArgumentOutOfRangeException(nameof(asset), asset.Class, "Unknown asset class.");
            }
        }

        private void BecomeIndependent()
        {
            _character.Finances.SetLifestyle(_settings.DefaultLifestyleId);
            if (_character.Family == null)
            {
                return;
            }
            int level = _character.Catalogs.FamilyBackgrounds.Get(_character.Family.BackgroundId).HouseholdIncomeLevel;
            if (_settings.FamilySupport.TryGetValue(level, out FamilySupportLevel support) && support.IndependenceGift > 0m)
            {
                _character.Wallet.Deposit(support.IndependenceGift);
                Report(FinanceLineCategory.Income, GiftKey, support.IndependenceGift);
            }
        }

        private void CollectIncome(GameDate week)
        {
            foreach (IIncomeSource source in _incomeSources)
            {
                foreach (IncomeItem item in source.MonthlyIncome(_character, week))
                {
                    Receive(item);
                }
            }
        }

        private void Receive(IncomeItem item)
        {
            _character.Wallet.Deposit(item.Amount);
            Report(FinanceLineCategory.Income, item.SourceKey, item.Amount);
            float taxRate = _character.LocationModifiers().TaxRate ?? 0f;
            decimal tax = item.Taxable ? Money.Round(item.Amount * (decimal)taxRate) : 0m;
            if (tax > 0m)
            {
                _character.Wallet.WithdrawUpTo(tax);
                Report(FinanceLineCategory.Tax, TaxKey, -tax);
            }
        }

        private void PayLivingCosts(GameDate week)
        {
            LifestyleTierDefinition tier = _lifestyles.Get(_character.Finances.LifestyleTierId);
            decimal cost = LifestyleCost(tier.Id);
            decimal paid = cost > 0m ? _character.Wallet.WithdrawUpTo(cost) : 0m;
            Report(FinanceLineCategory.Living, tier.NameKey, -cost);
            if (paid < cost)
            {
                CoverShortfall(cost - paid, tier.NameKey, week);
            }
            foreach (StatAmount amount in tier.MonthlyStats)
            {
                _character.ApplyStatChange(new StatChange(amount.StatId, amount.Amount, tier.NameKey), week);
            }
        }

        private void ServiceDebts(MarketMonth market, GameDate week, TickContext tick)
        {
            bool allOnTime = true;
            bool anyDue = false;
            foreach (Debt debt in _character.Finances.Debts.Debts.ToList())
            {
                DebtProductDefinition product = _products.Get(debt.ProductId);
                if (MonthsSince(debt.StartedOn, week) < product.GraceMonths || debt.Balance <= 0m)
                {
                    continue;
                }

                decimal interest = Money.Round(debt.Balance * (decimal)debt.AnnualRate / Money.MonthsPerYear);
                debt.Add(interest, _economy.Settings.MaxDebtBalance);
                Report(FinanceLineCategory.Interest, product.NameKey, -interest);

                decimal due = product.IsRevolving
                    ? Math.Min(debt.Balance, Math.Max(product.MinPayment, Money.Round(debt.Balance * (decimal)product.MinPaymentRate)))
                    : Math.Min(debt.Balance, debt.MonthlyPayment);
                anyDue = true;
                if (due <= 0m || _character.Wallet.TryWithdraw(due))
                {
                    debt.Pay(due);
                    debt.CountInstallment();
                    Report(FinanceLineCategory.DebtPayment, product.NameKey, -due);
                    CloseIfPaid(debt);
                    continue;
                }

                allOnTime = false;
                debt.Miss();
                debt.Add(product.LateFee, _economy.Settings.MaxDebtBalance);
                Report(FinanceLineCategory.Interest, product.NameKey, -product.LateFee);
                ChangeCredit(-_settings.Credit.MissedLoss, MissedKey, week);
                if (!debt.IsDefaulted && debt.MissedPayments >= _settings.DefaultAfterMissed)
                {
                    Default(debt, product, week, tick);
                }
            }
            if (anyDue && allOnTime)
            {
                ChangeCredit(_settings.Credit.OnTimeGain, OnTimeKey, week);
            }
        }

        private void Default(Debt debt, DebtProductDefinition product, GameDate week, TickContext tick)
        {
            debt.MarkDefaulted();
            ChangeCredit(-_settings.Credit.DefaultLoss, DefaultKey, week);
            _bus.Publish(new DebtDefaultedEvent(debt, week));
            tick.RequestInterrupt(Severity.Major, TroubleInterruptKey, product.Id);
            if (!debt.CollateralHoldingId.HasValue)
            {
                return;
            }

            Holding collateral = _character.Finances.Portfolio.Find(debt.CollateralHoldingId.Value);
            debt.ReleaseCollateral();
            if (collateral == null)
            {
                return;
            }
            decimal proceeds = Money.Round(collateral.Value * (1m - _settings.ForcedSaleDiscount));
            _character.Finances.Portfolio.Remove(collateral.Id);
            decimal toDebt = Math.Min(proceeds, debt.Balance);
            debt.Pay(toDebt);
            if (proceeds > toDebt)
            {
                _character.Wallet.Deposit(proceeds - toDebt);
            }
            ChangeCredit(-_settings.Credit.ForeclosureLoss, ForeclosureKey, week);
            _bus.Publish(new ForeclosureEvent(debt, collateral.AssetId, proceeds, week));
            CloseIfPaid(debt);
        }

        private void CheckBankruptcy(GameDate week, TickContext tick)
        {
            CharacterFinances finances = _character.Finances;
            bool defaultedUnsecured = finances.Debts.Debts.Any(d => d.IsDefaulted && _products.Get(d.ProductId).Dischargeable);
            if (finances.IsBankrupt(week) || !defaultedUnsecured || _character.NetWorth >= _settings.BankruptcyNetWorth)
            {
                return;
            }

            decimal pool = 0m;
            foreach (Holding holding in finances.Portfolio.Holdings.ToList())
            {
                pool += Money.Round(holding.Value * (1m - _settings.ForcedSaleDiscount));
                finances.Portfolio.Remove(holding.Id);
            }
            if (_character.Wallet.Cash > 0m)
            {
                pool += _character.Wallet.WithdrawUpTo(_character.Wallet.Cash);
            }
            decimal discharged = 0m;
            foreach (Debt debt in finances.Debts.Debts.Where(d => _products.Get(d.ProductId).Dischargeable).ToList())
            {
                decimal paid = Math.Min(pool, debt.Balance);
                pool -= paid;
                discharged += debt.Balance - paid;
                finances.Debts.Remove(debt.Id);
            }
            foreach (Debt debt in finances.Debts.Debts)
            {
                debt.ReleaseCollateral();
            }
            if (pool > 0m)
            {
                _character.Wallet.Deposit(pool);
            }

            GameDate until = week.AddWeeks(_settings.BankruptcyWeeks);
            finances.SetBankruptUntil(until);
            ChangeCredit(_settings.Credit.BankruptcyScore - _character.Stats.GetValue(StatIds.CreditScore), BankruptcyKey, week);
            SetFlag(BankruptFlag, week, _settings.BankruptcyWeeks);
            _bus.Publish(new BankruptcyEvent(discharged, until));
            tick.RequestInterrupt(Severity.Major, TroubleInterruptKey, BankruptFlag);
        }

        private void ApplyDebtStress(GameDate week)
        {
            decimal owed = _character.Finances.Debts.TotalBalance;
            decimal yearOfCosts = LifestyleCost(_settings.ReferenceLifestyleId) * Money.MonthsPerYear;
            if (owed <= 0m || yearOfCosts <= 0m)
            {
                return;
            }
            float stress = Math.Min(_settings.DebtStressMax, (float)(owed / yearOfCosts) * _settings.DebtStressPerYearOfCosts);
            if (_character.Finances.Debts.Debts.Any(d => d.Kind == DebtKind.Informal && d.Balance > 0m))
            {
                stress += _settings.InformalDebtStress;
            }
            _character.ApplyStatChange(new StatChange(StatIds.Stress, stress, DebtStressKey), week);
        }

        private void ApplyCardUtilization(GameDate week)
        {
            bool high = _character.Finances.Debts.Debts.Any(d => d.CreditLimit > 0m && d.Balance / d.CreditLimit > (decimal)_settings.Credit.HighUtilization);
            if (high)
            {
                ChangeCredit(-_settings.Credit.UtilizationLoss, UtilizationKey, week);
            }
        }

        private void UpdateWealth(GameDate week)
        {
            decimal monthCost = LifestyleCost(_settings.ReferenceLifestyleId);
            if (monthCost <= 0m)
            {
                return;
            }
            float target = _settings.WealthCurve.ScoreFor((float)(_character.NetWorth / monthCost));
            float delta = target - _character.Stats.GetValue(StatIds.Wealth);
            if (delta != 0f)
            {
                _character.ApplyStatChange(new StatChange(StatIds.Wealth, delta, NetWorthKey), week);
            }
        }

        private void ChangeCredit(float delta, string sourceKey, GameDate week)
        {
            if (delta != 0f)
            {
                _character.ApplyStatChange(new StatChange(StatIds.CreditScore, delta, sourceKey), week);
            }
        }

        private Debt AddDebt(DebtProductDefinition product, decimal principal, float rate, decimal payment, decimal limit, int? collateral)
        {
            return _character.Finances.Debts.Add(id => new Debt(id, product.Id, product.Kind, Money.Round(principal), rate, payment,
                product.TermMonths, limit, collateral, _clock.Now));
        }

        private LoanQuote RequireApproved(string productId, decimal amount)
        {
            LoanQuote quote = Quote(productId, amount);
            if (!quote.Approved)
            {
                throw new InvalidOperationException("Loan refused: " + string.Join(", ", quote.ReasonKeys));
            }
            return quote;
        }

        private void CloseIfPaid(Debt debt)
        {
            if (debt.Balance <= 0m && debt.CreditLimit <= 0m && debt.Kind != DebtKind.Informal || debt.Balance <= 0m && debt.Kind == DebtKind.Informal)
            {
                _character.Finances.Debts.Remove(debt.Id);
            }
        }

        private void RequireIndependent()
        {
            if (!IsIndependent(_clock.Now))
            {
                throw new InvalidOperationException(LoanQuote.ReasonTooYoung);
            }
        }

        private void Report(FinanceLineCategory category, string sourceKey, decimal amount)
        {
            _openReport?.Add(new FinanceLine(category, sourceKey, amount));
        }

        private void SetFlag(string flagId, GameDate week, int? expiryWeeks)
        {
            if (_character.Catalogs.Flags.Contains(flagId))
            {
                _character.Flags.Set(flagId, week, expiryWeeks);
            }
        }

        private void ClearFlag(string flagId)
        {
            if (_character.Catalogs.Flags.Contains(flagId) && _character.Flags.IsSet(flagId))
            {
                _character.Flags.Clear(flagId);
            }
        }

        private static decimal Installment(decimal principal, float annualRate, int months)
        {
            if (principal <= 0m || months <= 0)
            {
                return 0m;
            }
            double r = annualRate / Money.MonthsPerYear;
            double payment = r == 0.0 ? (double)principal / months : (double)principal * r / (1.0 - Math.Pow(1.0 + r, -months));
            return Money.Round((decimal)payment);
        }

        private static int MonthsSince(GameDate start, GameDate now)
        {
            return (now.Year - start.Year) * Money.MonthsPerYear + now.Month - start.Month;
        }
    }
}
