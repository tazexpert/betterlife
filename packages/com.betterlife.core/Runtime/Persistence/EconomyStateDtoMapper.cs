using System;
using System.Globalization;
using BetterLife.Core.Economy;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Converts <see cref="EconomyState"/> to and from <see cref="EconomyStateDto"/>.
    /// </summary>
    public static class EconomyStateDtoMapper
    {
        /// <summary>
        /// Captures the economy.
        /// </summary>
        /// <param name="state">State.</param>
        /// <returns>The DTO.</returns>
        public static EconomyStateDto ToDto(EconomyState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }
            return new EconomyStateDto
            {
                PhaseId = state.PhaseId,
                YearsLeftInPhase = state.YearsLeftInPhase,
                AnnualInflation = state.AnnualInflation,
                PolicyRate = state.PolicyRate,
                PriceLevel = state.PriceLevel.ToString(CultureInfo.InvariantCulture),
            };
        }

        /// <summary>
        /// Rebuilds the economy. An unknown phase (removed from content) restarts at the first phase.
        /// </summary>
        /// <param name="dto">Saved economy.</param>
        /// <param name="settings">Current economy settings.</param>
        /// <returns>The state.</returns>
        public static EconomyState FromDto(EconomyStateDto dto, EconomySettings settings)
        {
            if (dto == null)
            {
                throw new ArgumentNullException(nameof(dto));
            }
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            decimal priceLevel = FinancesDtoMapper.ParseMoney(dto.PriceLevel, nameof(dto.PriceLevel));
            string phaseId = settings.Phases.Contains(dto.PhaseId ?? string.Empty) ? dto.PhaseId : settings.Phases.All[0].Id;
            try
            {
                return new EconomyState(phaseId, Math.Max(1, dto.YearsLeftInPhase), dto.AnnualInflation, dto.PolicyRate, priceLevel);
            }
            catch (ArgumentException ex)
            {
                throw new FormatException("Saved economy is invalid: " + ex.Message, ex);
            }
        }
    }
}
