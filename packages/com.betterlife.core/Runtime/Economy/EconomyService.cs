using System;
using BetterLife.Core.Randomness;
using BetterLife.Core.Rules;

namespace BetterLife.Core.Economy
{
    /// <summary>
    /// Moves the macro economy: yearly phase changes, inflation and policy rate; monthly market returns.
    /// </summary>
    public sealed class EconomyService
    {
        private const decimal MinimumReturn = -0.95m;

        private readonly IRandomProvider _random;

        /// <summary>
        /// Creates the service.
        /// </summary>
        /// <param name="settings">Economy settings.</param>
        /// <param name="state">Economy state of the life.</param>
        /// <param name="random">Random stream for the economy.</param>
        public EconomyService(EconomySettings settings, EconomyState state, IRandomProvider random)
        {
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            State = state ?? throw new ArgumentNullException(nameof(state));
            _random = random ?? throw new ArgumentNullException(nameof(random));
            Settings.Phases.Get(state.PhaseId);
        }

        /// <summary>Economy settings.</summary>
        public EconomySettings Settings { get; }

        /// <summary>Economy state.</summary>
        public EconomyState State { get; }

        /// <summary>The current phase.</summary>
        public EconomyPhaseDefinition Phase => Settings.Phases.Get(State.PhaseId);

        /// <summary>
        /// Creates the economy of a life starting in a year: a random phase, rolled rates, and the era's price level.
        /// </summary>
        /// <param name="settings">Economy settings.</param>
        /// <param name="year">Starting year.</param>
        /// <param name="random">Random stream for the economy.</param>
        /// <returns>The state.</returns>
        public static EconomyState CreateInitial(EconomySettings settings, int year, IRandomProvider random)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            EconomyPhaseDefinition phase = settings.Phases.All[random.NextInt(0, settings.Phases.All.Count)];
            decimal priceLevel = (decimal)Math.Pow(1.0 + settings.HistoricalInflation, year - settings.BaseYear);
            return new EconomyState(phase.Id, random.NextInt(phase.MinYears, phase.MaxYears + 1),
                phase.Inflation.Roll(random), phase.PolicyRate.Roll(random), priceLevel);
        }

        /// <summary>
        /// Closes the year: prices rise by its inflation, the phase may change, and new rates are rolled.
        /// </summary>
        public void AdvanceYear()
        {
            decimal priceLevel = State.PriceLevel * (1m + (decimal)State.AnnualInflation);
            EconomyPhaseDefinition phase = Phase;
            int yearsLeft = State.YearsLeftInPhase - 1;
            if (yearsLeft < 1)
            {
                phase = Settings.Phases.Get(phase.NextId);
                yearsLeft = _random.NextInt(phase.MinYears, phase.MaxYears + 1);
            }
            State.Set(phase.Id, yearsLeft, phase.Inflation.Roll(_random), phase.PolicyRate.Roll(_random), priceLevel);
        }

        /// <summary>
        /// Rolls this month's market returns (always three values, in a fixed order).
        /// </summary>
        /// <returns>The month.</returns>
        public MarketMonth RollMonth()
        {
            EconomyPhaseDefinition phase = Phase;
            return new MarketMonth(
                Monthly(phase.Stocks),
                Monthly(phase.Property),
                Monthly(phase.Crypto),
                (decimal)Money.Monthly(State.AnnualInflation),
                State.PolicyRate);
        }

        /// <summary>
        /// Rolls a stock's own monthly return on top of the market.
        /// </summary>
        /// <param name="market">Market month.</param>
        /// <param name="beta">Sensitivity to the market.</param>
        /// <param name="annualVolatility">Company volatility.</param>
        /// <returns>Monthly real return.</returns>
        public decimal StockReturn(MarketMonth market, float beta, float annualVolatility)
        {
            float own = _random.NextGaussian(0f, annualVolatility / (float)Math.Sqrt(Money.MonthsPerYear));
            return Clamp(market.Stocks * (decimal)beta + (decimal)own);
        }

        private decimal Monthly(MarketParameters market)
        {
            float sample = _random.NextGaussian(market.Mean / Money.MonthsPerYear, market.Volatility / (float)Math.Sqrt(Money.MonthsPerYear));
            return Clamp((decimal)sample);
        }

        private static decimal Clamp(decimal rate) => Math.Max(MinimumReturn, rate);
    }
}
