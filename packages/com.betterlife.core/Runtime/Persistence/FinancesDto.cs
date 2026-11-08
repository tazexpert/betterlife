using System;
using System.Collections.Generic;

namespace BetterLife.Core.Persistence
{
    /// <summary>Save form of a character's personal finances.</summary>
    [Serializable]
    public sealed class FinancesDto
    {
        /// <summary>Lifestyle tier id, or null while dependent.</summary>
        public string LifestyleTierId;

        /// <summary>Whether BankruptUntilWeek is meaningful.</summary>
        public bool IsBankrupt;

        /// <summary>End of bankruptcy (GameDate.TotalWeeks) when IsBankrupt.</summary>
        public int BankruptUntilWeek;

        /// <summary>Last month's gross income as an invariant-culture decimal string.</summary>
        public string LastMonthlyIncome;

        /// <summary>Investments.</summary>
        public List<HoldingDto> Holdings = new List<HoldingDto>();

        /// <summary>Loans and credit lines.</summary>
        public List<DebtDto> Debts = new List<DebtDto>();

        /// <summary>Next holding id.</summary>
        public int NextHoldingId = 1;

        /// <summary>Next loan id.</summary>
        public int NextDebtId = 1;
    }
}
