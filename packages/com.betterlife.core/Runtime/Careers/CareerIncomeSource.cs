using System;
using System.Collections.Generic;
using BetterLife.Core.Domain;
using BetterLife.Core.Domain.Characters;
using BetterLife.Core.Domain.Stats;
using BetterLife.Core.Economy;
using BetterLife.Core.Randomness;
using BetterLife.Core.Time;

namespace BetterLife.Core.Careers
{
    /// <summary>
    /// Pays the current job each month: a fixed salary, or for volatile careers
    /// salary × max(0, 1 + noise + beta × the phase's expected stock return) × (0.5 + performance/100).
    /// </summary>
    public sealed class CareerIncomeSource : IIncomeSource
    {
        private const float PerformanceFloor = 0.5f;

        private readonly DefinitionCatalog<CareerTrackDefinition> _tracks;
        private readonly EconomyService _economy;
        private readonly IRandomProvider _random;

        /// <summary>
        /// Creates the source.
        /// </summary>
        /// <param name="tracks">Career tracks.</param>
        /// <param name="economy">Economy (outlook for business income).</param>
        /// <param name="random">Random stream for income noise.</param>
        public CareerIncomeSource(DefinitionCatalog<CareerTrackDefinition> tracks, EconomyService economy, IRandomProvider random)
        {
            _tracks = tracks ?? throw new ArgumentNullException(nameof(tracks));
            _economy = economy ?? throw new ArgumentNullException(nameof(economy));
            _random = random ?? throw new ArgumentNullException(nameof(random));
        }

        /// <inheritdoc />
        public IEnumerable<IncomeItem> MonthlyIncome(Character character, GameDate week)
        {
            Job job = character?.Career.Current;
            if (job == null)
            {
                yield break;
            }
            CareerTrackDefinition track = _tracks.Get(job.TrackId);
            decimal salary = track.Levels[job.LevelIndex].MonthlySalary;
            if (track.HasVariableIncome)
            {
                float noise = track.IncomeVolatility > 0f ? _random.NextGaussian(0f, track.IncomeVolatility) : 0f;
                float factor = Math.Max(0f, 1f + noise + track.MarketBeta * _economy.Phase.Stocks.Mean)
                    * (PerformanceFloor + job.Performance / StatBounds.Max);
                salary = Money.Round(salary * (decimal)factor);
            }
            if (salary > 0m)
            {
                yield return new IncomeItem(track.NameKey, salary, true);
            }
        }
    }
}
