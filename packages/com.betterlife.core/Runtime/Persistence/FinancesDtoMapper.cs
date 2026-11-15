using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using BetterLife.Core.Economy;
using BetterLife.Core.Time;

namespace BetterLife.Core.Persistence
{
    /// <summary>
    /// Converts <see cref="CharacterFinances"/> to and from <see cref="FinancesDto"/>, and drops entries whose content
    /// (assets, loan products, lifestyle tiers) no longer exists.
    /// </summary>
    public static class FinancesDtoMapper
    {
        /// <summary>
        /// Captures a character's finances.
        /// </summary>
        /// <param name="finances">Finances.</param>
        /// <returns>The DTO.</returns>
        public static FinancesDto ToDto(CharacterFinances finances)
        {
            if (finances == null)
            {
                throw new ArgumentNullException(nameof(finances));
            }
            return new FinancesDto
            {
                LifestyleTierId = finances.LifestyleTierId,
                IsBankrupt = finances.BankruptUntil.HasValue,
                BankruptUntilWeek = finances.BankruptUntil.HasValue ? finances.BankruptUntil.Value.TotalWeeks : 0,
                LastMonthlyIncome = Text(finances.LastMonthlyIncome),
                Holdings = finances.Portfolio.Holdings.Select(h => new HoldingDto
                {
                    Id = h.Id,
                    AssetId = h.AssetId,
                    Value = Text(h.Value),
                    CostBasis = Text(h.CostBasis),
                    AcquiredWeek = h.AcquiredOn.TotalWeeks,
                }).ToList(),
                Debts = finances.Debts.Debts.Select(d => new DebtDto
                {
                    Id = d.Id,
                    ProductId = d.ProductId,
                    Kind = d.Kind.ToString(),
                    Balance = Text(d.Balance),
                    AnnualRate = d.AnnualRate,
                    MonthlyPayment = Text(d.MonthlyPayment),
                    MonthsRemaining = d.MonthsRemaining,
                    CreditLimit = Text(d.CreditLimit),
                    HasCollateral = d.CollateralHoldingId.HasValue,
                    CollateralHoldingId = d.CollateralHoldingId ?? 0,
                    MissedPayments = d.MissedPayments,
                    Defaulted = d.IsDefaulted,
                    StartedWeek = d.StartedOn.TotalWeeks,
                }).ToList(),
                NextHoldingId = finances.Portfolio.NextId,
                NextDebtId = finances.Debts.NextId,
            };
        }

        /// <summary>
        /// Restores saved finances into an empty <see cref="CharacterFinances"/>. A null DTO (an older save) leaves them empty.
        /// </summary>
        /// <param name="dto">Saved finances, or null.</param>
        /// <param name="finances">Target.</param>
        public static void Restore(FinancesDto dto, CharacterFinances finances)
        {
            if (finances == null)
            {
                throw new ArgumentNullException(nameof(finances));
            }
            if (dto == null)
            {
                return;
            }
            try
            {
                finances.RestoreLifestyle(dto.LifestyleTierId);
                finances.SetBankruptUntil(dto.IsBankrupt ? new GameDate(dto.BankruptUntilWeek) : (GameDate?)null);
                finances.SetLastMonthlyIncome(dto.LastMonthlyIncome == null ? 0m : ParseMoney(dto.LastMonthlyIncome, nameof(dto.LastMonthlyIncome)));
                finances.Portfolio.Restore((dto.Holdings ?? new List<HoldingDto>()).Where(h => h != null).Select(h => new Holding(h.Id, h.AssetId,
                    ParseMoney(h.Value, nameof(h.Value)), ParseMoney(h.CostBasis, nameof(h.CostBasis)), new GameDate(h.AcquiredWeek))).ToList(), dto.NextHoldingId);
                finances.Debts.Restore((dto.Debts ?? new List<DebtDto>()).Where(d => d != null).Select(ToDebt).ToList(), dto.NextDebtId);
            }
            catch (ArgumentException ex)
            {
                throw new FormatException("Saved finances are invalid: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Removes holdings, loans and the lifestyle whose content no longer exists; returns one warning per removal.
        /// Loans secured by a removed holding lose their collateral.
        /// </summary>
        /// <param name="finances">Restored finances.</param>
        /// <param name="assetExists">True for known asset ids.</param>
        /// <param name="productExists">True for known loan product ids.</param>
        /// <param name="lifestyleExists">True for known lifestyle tier ids.</param>
        /// <returns>Warnings.</returns>
        public static IReadOnlyList<string> DropUnknown(CharacterFinances finances, Func<string, bool> assetExists,
            Func<string, bool> productExists, Func<string, bool> lifestyleExists)
        {
            if (finances == null || assetExists == null || productExists == null || lifestyleExists == null)
            {
                throw new ArgumentNullException(nameof(finances));
            }
            var warnings = new List<string>();
            foreach (Holding holding in finances.Portfolio.Holdings.Where(h => !assetExists(h.AssetId)).ToList())
            {
                finances.Portfolio.Remove(holding.Id);
                foreach (Debt debt in finances.Debts.Debts.Where(d => d.CollateralHoldingId == holding.Id))
                {
                    debt.ReleaseCollateral();
                }
                warnings.Add($"Unknown asset '{holding.AssetId}' removed from the portfolio.");
            }
            foreach (Debt debt in finances.Debts.Debts.Where(d => !productExists(d.ProductId)).ToList())
            {
                finances.Debts.Remove(debt.Id);
                warnings.Add($"Unknown loan product '{debt.ProductId}' removed.");
            }
            if (finances.LifestyleTierId != null && !lifestyleExists(finances.LifestyleTierId))
            {
                warnings.Add($"Unknown lifestyle '{finances.LifestyleTierId}' reset.");
                finances.RestoreLifestyle(null);
            }
            return warnings;
        }

        /// <summary>
        /// Parses an invariant-culture decimal string.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="field">Field name for the error.</param>
        /// <returns>The value.</returns>
        public static decimal ParseMoney(string text, string field)
        {
            if (!decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal value))
            {
                throw new FormatException($"Field '{field}' has invalid value '{text}'.");
            }
            return value;
        }

        private static Debt ToDebt(DebtDto dto)
        {
            if (!Enum.TryParse(dto.Kind, false, out DebtKind kind) || !Enum.IsDefined(typeof(DebtKind), kind))
            {
                throw new FormatException($"Field 'Kind' has invalid value '{dto.Kind}'.");
            }
            var debt = new Debt(dto.Id, dto.ProductId, kind, ParseMoney(dto.Balance, nameof(dto.Balance)), dto.AnnualRate,
                ParseMoney(dto.MonthlyPayment, nameof(dto.MonthlyPayment)), dto.MonthsRemaining, ParseMoney(dto.CreditLimit, nameof(dto.CreditLimit)),
                dto.HasCollateral ? dto.CollateralHoldingId : (int?)null, new GameDate(dto.StartedWeek));
            debt.RestoreStatus(dto.MissedPayments, dto.Defaulted);
            return debt;
        }

        private static string Text(decimal value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
